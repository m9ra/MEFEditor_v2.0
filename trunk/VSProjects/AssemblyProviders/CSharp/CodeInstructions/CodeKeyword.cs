using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AssemblyProviders.CSharp.Interfaces;
using AssemblyProviders.CSharp.Primitives;

namespace AssemblyProviders.CSharp.CodeInstructions
{
    class CodeKeyword:ICodeKeyword
    {
        public CodeKeyword(INodeAST keywordNode)
        {

            if (keywordNode.NodeType == NodeTypes.keyword)
                Keyword = keywordNode.Value;
            else
                Keyword = "nop";

            End = NodeTools.NodeEnding(keywordNode);
            Start = NodeTools.NodeStart(keywordNode);
        }
        public string Keyword{get;private set;}
        public NodeKind Kind { get { return NodeKind.keyword; } }
        public IPosition End { get; private set; }
        public IPosition Start { get; private set; }
    }
}
