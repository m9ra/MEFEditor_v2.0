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
            var source = new Source(code);
            var parsed=parser.Parse(source);
            _methods.Add(method, parsed);
        }

        protected void generateMethod(MethodID method, InstanceInfo[] argumentInfo, EmitterBase emitter)
        {
            throw new NotImplementedException();
        }
    }
}
