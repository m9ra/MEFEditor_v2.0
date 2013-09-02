using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeSystem;
using AssemblyProviders.CSharp.Interfaces;

namespace AssemblyProviders.CSharp.Compiling
{
    public class CompilationInfo
    {
        List<VariableInfo> _variables = new List<VariableInfo>();

        internal IEnumerable<VariableInfo> Variables { get { return _variables; } }

        internal void AddVariable(VariableInfo variable)
        {
            _variables.Add(variable);
        }

        private void checkNodeType(INodeAST node, NodeTypes nodeType)
        {
            if (node.NodeType != nodeType)
            {
                throw new NotSupportedException("Node is not of expected NodeType");
            }
        }
    }
}
