using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AssemblyProviders.CSharp.Interfaces;
using AssemblyProviders.CSharp.Primitives;

namespace AssemblyProviders.CSharp.CodeInstructions
{
    class CodeThis:ICodeThis
    {        
        public CodeThis(bool isStatic, INodeAST node)
        {            
            this.IsStatic = isStatic;

            End=NodeTools.NodeEnding(node);
            Start = NodeTools.NodeStart(node);
        }
        public bool IsStatic { get; private set; }

        public NodeKind Kind{get{return NodeKind.thisObj;}}


        public IPosition End { get; private set; }
        public IPosition Start { get; private set; }
    }
}
