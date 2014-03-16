using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeSystem
{
    /// <summary>
    /// Standard implementation of assembly collection
    /// </summary>
    public class AssemblyCollection : AssemblyCollectionBase
    {
        public AssemblyCollection(params AssemblyProvider[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                Add(assembly);
            }
        }

        public void Add(AssemblyProvider assembly)
        {
            addAssembly(assembly);
        }

        public void Remove(AssemblyProvider assembly)
        {
            removeAssembly(assembly);
        }
    }
}
