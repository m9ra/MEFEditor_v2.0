using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using TypeExperiments.Core;

namespace TypeExperiments.TypeBuilding
{
    class TypeOverride<ToOverride>
    {
        public const string OVERRIDE_PREFIX = "_call_";

        protected ToOverride This;

        /// <summary>
        /// Register overriden type into assembly
        /// </summary>
        /// <param name="assembly"></param>
        public void RegisterInto(InternalAssembly assembly){        
            var toOverrideType=typeof(ToOverride);
            var overrides = collectMethods(toOverrideType, "_call_");
            var originalMethods = collectMethods(toOverrideType);

            var overridenMethods = getOverriden(originalMethods, overrides);

        }

        private MethodInfo[] collectMethods(Type type, string methodPrefix=null)
        {
            throw new NotImplementedException();
        }

        private MethodInfo[] getOverriden(IEnumerable<MethodInfo> originalMethods, IEnumerable<MethodInfo> overrides)
        {
            throw new NotImplementedException();
        }

  
    }
}
