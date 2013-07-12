using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using AssemblyProviders.CSharp.Interfaces;
using AssemblyProviders.CSharp.Primitives;

namespace AssemblyProviders.CSharp.CodeInstructions
{

    class ArgInfo
    {
        public int ArgPos;
        public string Name;
        public ArgInfo(int argpos, string name)
        {
            ArgPos = argpos;
            Name = name;
        }
    }

    class CodeArgument:ICodeArgument
    {
        public CodeArgument(ArgInfo info, INodeAST sourceNode)
        {
            this.Name = info.Name;
            this.ArgPos = info.ArgPos;

            End = NodeTools.NodeEnding(sourceNode);
            Start = NodeTools.NodeStart(sourceNode);
        }
        public string Name { get; private set; }
        public int ArgPos { get; private set; }
        public bool IsStatic { get { return false; } }

        public NodeKind Kind { get { return NodeKind.argument; } }

        public IPosition End { get; private set; }
        public IPosition Start { get; private set; }
    }
}
