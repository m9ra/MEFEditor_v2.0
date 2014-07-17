using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeSystem.Core;

namespace TypeSystem
{
    /// <summary>
    /// Runtime representation of assembly, that can be used by <see cref="RuntimeTypeDefinition"/> implementations.
    /// </summary>
    public class TypeAssembly
    {
        /// <summary>
        /// Manager providing type system services
        /// </summary>
        private readonly AssembliesManager _manager;

        /// <summary>
        /// Provider of represented assembly
        /// </summary>
        private readonly AssemblyProvider _assembly;

        /// <summary>
        /// Name of represented assembly
        /// </summary>
        public string Name { get { return _assembly.Name; } }

        internal TypeAssembly(AssembliesManager manager, AssemblyProvider assembly)
        {
            _manager = manager;
            _assembly = assembly;
        }

        /// <summary>
        /// Get components defined by represented assembly
        /// </summary>
        /// <returns>Components defined by represented assembly</returns>
        public IEnumerable<ComponentInfo> GetComponents()
        {
            return _manager.GetComponents(_assembly);
        }

        /// <summary>
        /// Get components defined by represented assembly and all its referenced assemblies.
        /// </summary>
        /// <returns>Components defined by represented assembly and all its referenced assemblies</returns>
        public IEnumerable<ComponentInfo> GetReferencedComponents()
        {
            return _manager.GetReferencedComponents(_assembly);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _assembly.Name.ToString();
        }
    }
}
