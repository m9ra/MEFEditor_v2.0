using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypeSystem.Core
{
    /// <summary>
    /// Representation of resolved assembly references
    /// </summary>
    class ReferencedAssemblies : IEnumerable<object>
    {
        /// <summary>
        /// Stored references
        /// </summary>
        private readonly List<object> _assemblyKeys = new List<object>();

        /// <summary>
        /// Add assemblyKey representing referenced assembly
        /// </summary>
        /// <param name="assemblyKey">Key of referenced assembly</param>
        internal void Add(object assemblyKey)
        {
            if (_assemblyKeys.Contains(assemblyKey))
                //assembly is already contained
                return;

            _assemblyKeys.Add(assemblyKey);
        }

        /// <summary>
        /// Remove given reference
        /// </summary>
        /// <param name="assemblyKey">Key representing removed assembly</param>
        internal void Remove(object assemblyKey)
        {
            _assemblyKeys.Remove(assemblyKey);
        }

        /// </ inheritdoc>
        public IEnumerator<object> GetEnumerator()
        {
            return _assemblyKeys.GetEnumerator();
        }

        /// </ inheritdoc>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _assemblyKeys.GetEnumerator();
        }
    }
}
