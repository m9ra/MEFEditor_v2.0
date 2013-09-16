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
        internal bool HasImports(Instance instance)
        {
            throw new NotImplementedException();
        }

        internal bool IsInstanceConstructed(Instance inst)
        {
            throw new NotImplementedException();
        }

        internal bool IsSubType(InstanceInfo expType, string importItem)
        {
            throw new NotImplementedException();
        }

        internal IEnumerable<TypeMethodInfo> GetMethods(InstanceInfo metadataType)
        {
            throw new NotImplementedException();
        }

        internal bool IsSubType(InstanceInfo testedType, InstanceInfo setterType)
        {
            throw new NotImplementedException();
        }

        internal IEnumerable<Export> GetExports(Instance instance)
        {
            throw new NotImplementedException();
        }

        internal IEnumerable<Export> GetSelfExports(Instance instance)
        {
            throw new NotImplementedException();
        }

        internal ComponentInfo GetComponentInfo(Instance inst)
        {
            throw new NotImplementedException();
        }

        internal void AddCall(Instance inst, MethodID constr, params Instance[] args)
        {
            throw new NotImplementedException();
        }

        internal Instance CreateArray(InstanceInfo instanceInfo, IEnumerable<Instance> instances)
        {
            throw new NotImplementedException();
        }

        internal IEnumerable<TypeMethodInfo> GetMethods(InstanceInfo instType, string getterName)
        {
            throw new NotImplementedException();
        }
    }



   


  

}
