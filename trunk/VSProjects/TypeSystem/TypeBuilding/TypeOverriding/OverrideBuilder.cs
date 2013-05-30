using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using TypeSystem.Core;

namespace TypeSystem.TypeBuilding.TypeOverriding
{
    class OverrideBuilder
    {
        TypeName _name;
        Dictionary<MethodInfo, MethodInfo> _overrides = new Dictionary<MethodInfo, MethodInfo>();

        public OverrideBuilder(TypeName name)
        {
            _name = name;
        }

        public void AddOverride(MethodInfo originalMethod, MethodInfo overrideMethod)
        {
            _overrides.Add(originalMethod, overrideMethod);
        }

        public InternalType Build(InternalAssembly assembly)
        {
            return new OverridenType(_overrides,_name, assembly);
        }
    }
}
