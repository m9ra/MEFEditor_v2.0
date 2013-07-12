using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyProviders.CSharp.Primitives
{
    class NodeException : ParsingException
    {
        private string p;
        private Interfaces.INodeAST nodeAST;

        public NodeException(string p, Interfaces.INodeAST nodeAST)
            : base(p)
        {
            // TODO: Complete member initialization
            this.p = p;
            this.nodeAST = nodeAST;
        }
    }

    class ParsingException : Exception
    {
        private string p;

        public ParsingException(string p)
        {
            // TODO: Complete member initialization
            this.p = p;
        }
    }
}
