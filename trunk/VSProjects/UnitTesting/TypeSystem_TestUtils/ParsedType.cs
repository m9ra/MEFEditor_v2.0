using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using TypeSystem;

using AssemblyProviders.CSharp.Primitives;

namespace AssemblyProviders.CSharp
{
    class ParsedType
    {
        static SyntaxParser parser = new SyntaxParser();


        Dictionary<MethodID, CodeNode> _methods = new Dictionary<MethodID, CodeNode>();

        ParsedType()
        {
        }

        void AddMethod(MethodID method, string code)
        {
            var parsed=parser.Parse(code);
            _methods.Add(method, parsed);
        }

        protected void generateMethod(MethodID method, InstanceInfo[] argumentInfo, IEmitter<MethodID,InstanceInfo> emitter)
        {
            throw new NotImplementedException();
        }
    }
}
