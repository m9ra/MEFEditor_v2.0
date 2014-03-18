using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypeSystem.Core
{
    /// <summary>
    /// Representation of assemblies resolved from references
    /// </summary>
    class ReferencedAssemblies:IEnumerable<AssemblyProvider>
    {
        /// <summary>
        /// Stored assemblies
        /// </summary>
        private readonly List<AssemblyProvider> _assemblies = new List<AssemblyProvider>();

        /// <summary>
        /// Add reference to given assembly
        /// </summary>
        /// <param name="assembly">Assembly to be referenced</param>
        internal void Add(AssemblyProvider assembly)
        {
            if (_assemblies.Contains(assembly))
                //assembly is already contained
                return;

            _assemblies.Add(assembly);
        }

        /// <summary>
        /// Remove given assembly from references
        /// </summary>
        /// <param name="assembly">Assembly to be removed from references</param>
        internal void Remove(AssemblyProvider assembly)
        {
            _assemblies.Remove(assembly);
        }

        /// </ inheritdoc>
        public IEnumerator<AssemblyProvider> GetEnumerator()
        {
            return _assemblies.GetEnumerator();
        }

        /// </ inheritdoc>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _assemblies.GetEnumerator();
        }
    }
}
