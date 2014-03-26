using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AssemblyProviders.CSharp.Interfaces;
namespace AssemblyProviders.CSharp
{
    /// <summary>
    /// Extensions for nodes
    /// </summary>
    static class NodeExtensions
    {
        /// <summary>
        /// Determine that current node represents assign.
        /// </summary>
        /// <param name="node">Tested node</param>
        /// <returns><c>true</c> for assign nodes, <c>false</c> otherwise</returns>
        public static bool IsAssign(this INodeAST node)
        {
            var v = node.Value;

            //note that => ,<=, >= are not assigns
            return
                v.EndsWith("=") &&
                !v.Contains('>') &&
                !v.Contains('<');
        }

        /// <summary>
        /// Get index of argument in argument list of current node 
        /// </summary>
        /// <param name="call">Call which argument is tested</param>
        /// <param name="argument">Argument which index is needed</param>
        /// <returns>Index of argument</returns>
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
