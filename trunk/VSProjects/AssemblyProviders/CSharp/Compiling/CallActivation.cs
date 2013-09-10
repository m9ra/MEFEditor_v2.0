using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeSystem;

using AssemblyProviders.CSharp.Interfaces;

namespace AssemblyProviders.CSharp.Compiling
{
    class CallActivation
    {
        private readonly List<RValueProvider> _arguments = new List<RValueProvider>();

        internal readonly TypeMethodInfo MethodInfo;

        internal INodeAST CallNode;

        internal IEnumerable<RValueProvider> Arguments{get{return _arguments;}}

        internal RValueProvider CalledObject { get; set; }

        internal CallActivation(TypeMethodInfo methodInfo)
        {
            MethodInfo = methodInfo;
        }

        internal void AddArgument(RValueProvider arg)
        {
            _arguments.Add(arg);
        }

        internal void SetCallNode(INodeAST callNode)
        {
            CallNode = callNode;
        }
    }
}
