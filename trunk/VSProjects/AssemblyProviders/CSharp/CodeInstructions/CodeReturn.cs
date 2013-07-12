using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AssemblyProviders.CSharp.Interfaces;
using AssemblyProviders.CSharp.Primitives;

namespace AssemblyProviders.CSharp.CodeInstructions
{
    class CodeReturn:ICodeReturn
    {
        public CodeReturn(ICodeValueProvider value,INodeAST node)
        {
            ReturnValue = value;
            End = NodeTools.NodeEnding(node);
            Start = NodeTools.NodeStart(node);
        }
        public ICodeValueProvider ReturnValue { get; private set; }
        public NodeKind Kind { get { return NodeKind.fReturn; } }

        public IPosition End { get; private set; }
        public IPosition Start { get; private set; }

        public bool IsStatic { get { return false; } }
    }
}
