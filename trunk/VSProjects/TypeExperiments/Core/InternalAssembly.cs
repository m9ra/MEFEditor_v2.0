using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeExperiments.Reflection;

namespace TypeExperiments.Core
{
    class InternalAssembly
    {
        Dictionary<TypeName, InternalType> _loadedTypes = new Dictionary<TypeName, InternalType>();
        internal void RegisterType(TypeName name, InternalType type)
        {
            _loadedTypes.Add(name, type);
        }

        public InternalType ResolveType(TypeName name)
        {
            return _loadedTypes[name];
        }
    }
}
