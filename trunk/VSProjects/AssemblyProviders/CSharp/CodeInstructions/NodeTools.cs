using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AssemblyProviders.CSharp.Interfaces;
using AssemblyProviders.CSharp.Primitives;

namespace AssemblyProviders.CSharp.CodeInstructions
{
    static class NodeTools
    {
        public static Position NodeEnding(INodeAST node)
        {            
            var next = node.EndingToken.Next;
            if (next == null)
            {
                var endPos = node.EndingToken.Position;
                return endPos.Shift(node.EndingToken.Value.Length);
            }
            
            return next.Position;
        }
        public static Position NodeStart(INodeAST node)
        {
            var start = node.StartingToken.Previous;
            if (start == null)
            {
                var startPos = node.StartingToken.Position;
                return startPos.Shift(node.StartingToken.Value.Length);
            }
            return start.Position.Shift(start.Value.Length);
        }

        internal static Position NodeBlockEnding(INodeAST node)
        {
            //blocks need double next
            var next = node.EndingToken.Next;
            if (next != null && next.Next!=null)
                next = next.Next;

            if (next == null)
                next = node.EndingToken;

            return next.Position;
        }
    }
}
