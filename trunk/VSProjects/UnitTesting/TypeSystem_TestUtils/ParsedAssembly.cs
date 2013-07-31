using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeSystem;

namespace UnitTesting.TypeSystem_TestUtils
{
    class ParsedAssembly : AssemblyProvider
    {
        Dictionary<string, ParsedGenerator> _methods = new Dictionary<string, ParsedGenerator>();

        internal ParsedAssembly AddMethod(string name, string source,bool isStatic=false)
        {
            _methods.Add(name, new ParsedGenerator("{" + source + "}"));

            return this;
        }

        protected override string resolveMethod(MethodID method, InstanceInfo[] staticArgumentInfo)
        {
            return method.MethodName;
        }

        protected override IInstructionGenerator getGenerator(string methodName)
        {
            var generator = _methods[methodName];
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
        readonly private Dictionary<string, ParsedGenerator> _methods;

        readonly string _actualPath;

        public HashIterator(Dictionary<string, ParsedGenerator> methods,string actualPath="")
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

            ParsedGenerator generator;
            if (_methods.TryGetValue(name, out generator))
            {
                yield return new TypeMethodInfo(_actualPath, searchedName);
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
