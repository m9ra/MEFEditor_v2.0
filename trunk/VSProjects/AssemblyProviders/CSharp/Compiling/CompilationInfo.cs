using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using TypeSystem;

using AssemblyProviders.CSharp.Interfaces;
using AssemblyProviders.CSharp.Transformations;

namespace AssemblyProviders.CSharp.Compiling
{
    public class CompilationInfo
    {
        List<VariableInfo> _variables = new List<VariableInfo>();
        Dictionary<INodeAST, InstanceInfo> _nodeTypes = new Dictionary<INodeAST, InstanceInfo>();
        Dictionary<INodeAST, CallProvider> _callProviders = new Dictionary<INodeAST, CallProvider>();

        internal IEnumerable<VariableInfo> Variables { get { return _variables; } }

        internal void RegisterCallProvider(INodeAST callNode, CallProvider callProvider)
        {
            _callProviders.Add(callNode, callProvider);
        }

        internal CallProvider GetProvider(INodeAST call)
        {
            return _callProviders[call];
        }

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

        internal void ReportNodeType(INodeAST valueNode, InstanceInfo instanceInfo)
        {
            _nodeTypes.Add(valueNode, instanceInfo);
        }

        internal InstanceInfo GetNodeType(INodeAST node)
        {
            InstanceInfo result;
            _nodeTypes.TryGetValue(node, out result);
            return result;  
        }
    }
}
