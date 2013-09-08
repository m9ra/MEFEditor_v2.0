using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeSystem;

namespace UnitTesting.TypeSystem_TestUtils
{
    class TestAssemblyCollection:AssemblyCollection
    {
        internal TestAssemblyCollection(params AssemblyProvider[] assemblies)
        {
            foreach (var assembly in assemblies) {
                Add(assembly);
            }
        }

        internal void Add(AssemblyProvider assembly)
        {
            addAssembly(assembly);
        }
    }
}
