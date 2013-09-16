using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using TypeSystem;
using Utilities;


namespace MEFAnalyzers.CompositionEngine
{

    public class CompositionContext
    {
        private readonly TypeServices _services;
        internal CompositionContext(TypeServices services)
        {
            _services = services;
        }

        internal bool IsSubType(InstanceInfo expType, string importItem)
        {
            throw new NotImplementedException();
        }

        internal bool IsSubType(InstanceInfo testedType, InstanceInfo setterType)
        {
            throw new NotImplementedException();
        }

        internal ComponentRef CreateArray(InstanceInfo instanceInfo, IEnumerable<ComponentRef> instances)
        {
            throw new NotImplementedException();
        }

        internal ComponentRef Register(Instance component)
        {
            var info = _services.GetComponentInfo(component);
            return new ComponentRef(this, component, info);
        }

        internal IEnumerable<TypeMethodInfo> GetMethods(InstanceInfo metadataType)
        {
            throw new NotImplementedException();
        }

        internal IEnumerable<TypeMethodInfo> GetMethods(InstanceInfo instType, string getterName)
        {
            throw new NotImplementedException();
        }
    }

}
