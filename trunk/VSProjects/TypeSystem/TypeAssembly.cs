using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeSystem.Core;

namespace TypeSystem
{
    public class TypeAssembly
    {
        private AssembliesManager _manager;
        private AssemblyProvider _assembly;

        internal TypeAssembly(AssembliesManager manager, AssemblyProvider assembly)
        {
            _manager = manager;
            _assembly = assembly;
        }

        public IEnumerable<ComponentInfo> GetComponents()
        {
            return _manager.GetComponents(_assembly);
        }

        public IEnumerable<ComponentInfo> GetReferencedComponents()
        {
            return _manager.GetReferencedComponents(_assembly);
        }

        public override string ToString()
        {
            return _assembly.Name.ToString();
        }
    }
}
