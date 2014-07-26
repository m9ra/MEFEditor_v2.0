using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;    

using TypeExperiments.Core;

namespace TypeExperiments.TypeBuilding.TypeOverriding
{
    /// <summary>
    /// WARNING: Overriden types shouldn't support Wrapping/Unwrapping - because of skipping override
    /// </summary>
    class OverridenType:InternalType
    {
        private Dictionary<MethodInfo, MethodInfo> _overrides;
        private TypeName _name;

        public OverridenType(Dictionary<MethodInfo, MethodInfo> overrides, TypeName name, InternalAssembly assembly):base(name,assembly)
        {            
            _overrides = overrides;
            _name = name;
        }

        internal override Instance ConstructInstance(params object[] args)
        {
        //    TypeUtils.
            throw new NotImplementedException();
        }

        protected override Instance _invoke(Instance thisInstance, string methodName, params Instance[] args)
        {
            var overridenInstance = thisInstance as OverridenInstance;
            var overridenObj = overridenInstance.OverridenObject;


      //      var methodInfo=

            return base._invoke(thisInstance, methodName, args);
        }
    }
}
