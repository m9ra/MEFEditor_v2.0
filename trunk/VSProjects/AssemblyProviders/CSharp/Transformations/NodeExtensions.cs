using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AssemblyProviders.CSharp.Interfaces;
namespace AssemblyProviders.CSharp.Transformations
{
    static class NodeExtensions
    {

        public static bool IsAssign(this INodeAST node)
        {
            //TODO math assigns
            return node.Value == "=";
        }
    }
}
