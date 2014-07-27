using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RecommendedExtensions.Core.Languages.CSharp.Interfaces;

namespace RecommendedExtensions.Core.Languages.CSharp
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
        /// Determine that current node represents constructor call.
        /// </summary>
        /// <param name="node">Tested node</param>
        /// <returns><c>true</c> for constructor nodes, <c>false</c> otherwise</returns>
        public static bool IsConstructor(this INodeAST node)
        {
            while (node != null)
            {
                if (node.Value == CSharpSyntax.NewOperator)
                    return true;

                node = node.Parent;
            }

            return false;
        }

        /// <summary>
        /// Determine that current node represents call root.
        /// </summary>
        /// <param name="node">Tested node</param>
        /// <returns><c>true</c> for call root node, <c>false</c> otherwise</returns>
        public static bool IsCallRoot(this INodeAST node)
        {
            var type = node.NodeType;

            if (type != NodeTypes.hierarchy || type != NodeTypes.call)
                //only hierarchy or call can be root of call
                return false;
            
            var current = node;
            while (current != null)
            {
                if (current.NodeType == NodeTypes.call)
                    //call hierarchy has to end by call
                    return true;

                current = current.Child;
            }

            return false;
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
