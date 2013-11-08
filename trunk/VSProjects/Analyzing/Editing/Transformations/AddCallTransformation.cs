using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Editing.Transformations
{
    class AddCallTransformation : Transformation
    {
        private readonly CallProvider _provider;

        private bool _canCommit;

        internal AddCallTransformation(CallProvider provider)
        {
            _provider = provider;
        }

        protected override void apply(TransformationServices services)
        {
            _canCommit = false;
            var call = _provider(services);
            if (call == null)
                return;

           
            var scopeTransform = new CommonScopeTransformation(call.Instances);
            services.Apply(scopeTransform);

            var instanceScopes = scopeTransform.InstanceScopes;
            if (scopeTransform.InstanceScopes == null)
            {
                services.Abort("Can't get instances to same scope");
                return;
            }

            var transformProvider = instanceScopes.ScopeBlock.Info.BlockTransformProvider;

            var subsitutedCall = call.Substitute(instanceScopes.InstanceVariables);
            var appendTransform = transformProvider.AppendCall(subsitutedCall);

            services.Apply(appendTransform);

            _canCommit = !services.IsAborted;
        }


        protected override bool commit()
        {
            return _canCommit;
        }

        private void appendInstance(object testedObj, List<Instance> instances)
        {
            var inst = testedObj as Instance;
            if (inst == null)
                return;

            instances.Add(inst);
        }
    }
}
