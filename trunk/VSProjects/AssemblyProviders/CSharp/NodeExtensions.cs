using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AssemblyProviders.CSharp.Interfaces;
namespace AssemblyProviders.CSharp
{
    static class NodeExtensions
    {

        public static bool IsAssign(this INodeAST node)
        {
            //TODO math assigns
            return node.Value == "=";
        }

        public static int GetArgumentIndex(this INodeAST call, INodeAST argument)
        {
            for (int i = 0; i < call.Arguments.Length; ++i)
            {
                if (call.Arguments[i] == argument)
                    return i;
            }
            throw new NotSupportedException("Given argument is not node of given call");
        }
    }
}
