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
    /// <summary>
    /// Represents detailed information about <see cref="Source"/> that has been collected during compilation.
    /// </summary>
    public class CompilationInfo
    {
        /// <summary>
        /// Mapping of nodes on type that has been resolved during compilation.
        /// </summary>
        Dictionary<INodeAST, TypeDescriptor> _nodeTypes = new Dictionary<INodeAST, TypeDescriptor>();

        /// <summary>
        /// Mapping of call providers according to corresponding <see cref="INodeAST"/>.
        /// </summary>
        Dictionary<INodeAST, CallProvider> _callProviders = new Dictionary<INodeAST, CallProvider>();

        /// <summary>
        /// Variables that are currently declared in method
        /// </summary>
        private readonly Dictionary<string, VariableInfo> _declaredVariables = new Dictionary<string, VariableInfo>();

        /// <summary>
        /// Enumeration of all variables that has been declared within the method        
        /// </summary>
        internal IEnumerable<VariableInfo> Variables { get { return _declaredVariables.Values; } }

        /// <summary>
        /// Register call provider for given node
        /// </summary>
        /// <param name="callNode">Node corresponding to given call provider</param>
        /// <param name="callProvider">Registered call provider</param>
        internal void RegisterCallProvider(INodeAST callNode, CallProvider callProvider)
        {
            _callProviders[callNode] = callProvider;
        }

        /// <summary>
        /// Get provider registered for given call
        /// </summary>
        /// <param name="call">Node which coresponding call provider is needed</param>
        /// <returns>Registered call provider if available, <c>null</c> otherwise</returns>
        internal CallProvider GetProvider(INodeAST call)
        {
            return _callProviders[call];
        }

        /// <summary>
        /// Declare variable according to given info
        /// </summary>
        /// <param name="variable">Declared variable</param>
        internal void DeclareVariable(VariableInfo variable)
        {
            /// TODO multiple declarations for variable can be made
            _declaredVariables.Add(variable.Name, variable);
        }

        /// <summary>
        /// Get <see cref="VariableInfo"/> for given name
        /// </summary>
        /// <param name="name">Name of searched variable</param>
        /// <returns><see cref="VariableInfo"/> for available for given name, <c>null</c> if variable is not declared</returns>
        internal VariableInfo TryGetVariable(string name)
        {
            VariableInfo variable;
            _declaredVariables.TryGetValue(name, out variable);

            return variable;
        }

        /// <summary>
        /// Register type that has been discovered for given value node
        /// </summary>
        /// <param name="valueNode">Node which value is of given type</param>
        /// <param name="type">Type of value represented by given node</param>
        internal void RegisterNodeType(INodeAST valueNode, TypeDescriptor type)
        {
            _nodeTypes.Add(valueNode, type);
        }

        /// <summary>
        /// Get type registered for value represented by given node
        /// </summary>
        /// <param name="node">Node which type is needed</param>
        /// <returns>Type of value represented by node if registered, <c>null</c> otherwise</returns>
        internal TypeDescriptor GetNodeType(INodeAST node)
        {
            TypeDescriptor result;
            _nodeTypes.TryGetValue(node, out result);
            return result;
        }

        /// <summary>
        /// Resolve type of value that is assigned into variable
        /// </summary>
        /// <param name="variableUsing">Using where variable is assigned</param>
        /// <returns><see cref="TypeDescriptor"/> of resolved type if available, <c>null</c> otherwise</returns>
        internal TypeDescriptor ResolveAssignType(INodeAST variableUsing)
        {
            if (variableUsing.Parent == null || 
                variableUsing.Parent.Arguments.Length < 2 || 
                !variableUsing.Parent.Value.Contains('='))
                return null;

            var assignedNode = variableUsing.Parent.Arguments[1];
            return resolveNodeType(assignedNode);
        }

        private TypeDescriptor resolveNodeType(INodeAST node)
        {
            return GetNodeType(node);
        }
    }
}
