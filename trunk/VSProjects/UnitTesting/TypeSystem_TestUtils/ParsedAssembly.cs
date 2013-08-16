using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeSystem;
using AssemblyProviders.CSharp.Compiling;

namespace UnitTesting.TypeSystem_TestUtils
{
    public class ParsedAssembly : AssemblyProvider
    {
        Dictionary<string, Tuple<TypeMethodInfo, ParsedGenerator>> _methods = new Dictionary<string, Tuple<TypeMethodInfo, ParsedGenerator>>();

        public ParsedAssembly AddMethod(string name, string source,bool isStatic=false,params ParameterInfo[] arguments)
        {
            var nameParts=name.Split('.');
            var methodName=nameParts.Last();
            var typeName =string.Join(".", nameParts.Take(nameParts.Count() - 1).ToArray());

            var method=Tuple.Create(new TypeMethodInfo(typeName,methodName,isStatic),new ParsedGenerator("{" + source + "}",arguments));
            _methods.Add(name,method );

            return this;
        }

        protected override string resolveMethod(MethodID method, InstanceInfo[] staticArgumentInfo)
        {
            return method.MethodName;
        }

        protected override IInstructionGenerator getGenerator(string methodName)
        {
            var generator = _methods[methodName].Item2;
            generator.SetServices(TypeServices);
            return generator;
        }

        public override SearchIterator CreateRootIterator()
        {
            return new HashIterator(_methods);
        }
    }

    class HashIterator : SearchIterator
    {        
        readonly private Dictionary<string, Tuple<TypeMethodInfo, ParsedGenerator>> _methods = new Dictionary<string, Tuple<TypeMethodInfo, ParsedGenerator>>();

        readonly string _actualPath;

        public HashIterator(Dictionary<string, Tuple<TypeMethodInfo, ParsedGenerator>> methods, string actualPath = "")
        {
            _methods = methods;
            _actualPath = actualPath;
        }

        public override SearchIterator ExtendName(string suffix)
        {
            return new HashIterator(_methods,extendPath(suffix));
        }

        public override IEnumerable<TypeMethodInfo> FindMethods(string searchedName)
        {
            var name = extendPath(searchedName);

            Tuple<TypeMethodInfo,ParsedGenerator> method;
            if (_methods.TryGetValue(name, out method))
            {
                yield return method.Item1;
            }
        }


        private string extendPath(string name)
        {
            if (_actualPath == "")
            {
                return name;
            }
            else
            {
               return string.Format("{0}.{1}", _actualPath, name);
            }
        }
    }
}
