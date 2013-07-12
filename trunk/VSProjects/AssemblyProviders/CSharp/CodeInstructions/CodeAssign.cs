using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AssemblyProviders.CSharp.Interfaces;
using AssemblyProviders.CSharp.Primitives;

namespace AssemblyProviders.CSharp.CodeInstructions
{
    class CodeAssign : ICodeAssign
    {
        public CodeAssign(ICodeLValue lValue, ICodeValueProvider valueProvider,INodeAST assignNode)
        {
            this.LValue = lValue;
            this.ValueProvider = valueProvider;
            End = NodeTools.NodeEnding(assignNode);
            Start = NodeTools.NodeStart(assignNode);
        }

        public ICodeLValue LValue { get; private set; }
        public ICodeValueProvider ValueProvider { get; private set; }

        public bool IsStatic { get { return false; } }

        public NodeKind Kind { get { return NodeKind.assign; } }


        public IPosition End { get; private set; }
        public IPosition Start { get; private set; }
    }
}
