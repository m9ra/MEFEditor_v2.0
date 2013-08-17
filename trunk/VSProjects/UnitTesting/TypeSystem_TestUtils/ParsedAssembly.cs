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
        Dictionary<string,  MethodItem> _methods = new Dictionary<string, MethodItem>();

        public ParsedAssembly AddMethod(string name, string source,bool isStatic=false,params ParameterInfo[] arguments)
        {
            var nameParts=name.Split('.');
            var methodName=nameParts.Last();
            var typeName =string.Join(".", nameParts.Take(nameParts.Count() - 1).ToArray());

            var info=new TypeMethodInfo(typeName,methodName,arguments,isStatic);
            var method=new ParsedGenerator(info,"{" + source + "}");
            _methods.Add(name,new MethodItem(method,info) );

            return this;
        }

        protected override string resolveMethod(MethodID method, InstanceInfo[] staticArgumentInfo)
        {
            return method.MethodName;
        }

        protected override IInstructionGenerator getGenerator(string methodName)
        {
            if (!_methods.ContainsKey(methodName))
            {
                return null;
            }
            var generator = _methods[methodName].Generator as ParsedGenerator;
            generator.SetServices(TypeServices);
            return generator;
        }

        public override SearchIterator CreateRootIterator()
        {
            return new HashIterator(_methods);
        }
    }
}
