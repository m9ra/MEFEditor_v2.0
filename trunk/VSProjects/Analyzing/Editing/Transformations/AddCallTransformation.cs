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

        protected override void apply(ExecutionView view)
        {
            _canCommit = false;
            var call = _provider(view);
            if (call == null)
                return;

            var scopeTransform = new CommonScopeTransformation(call.Instances);
            view.Apply(scopeTransform);

            var instanceScopes = scopeTransform.InstanceScopes;
            if (scopeTransform.InstanceScopes == null)
            {
                view.Abort("Can't get instances to same scope");
                return;
            }

            var subsitutedCall = call.Substitute(instanceScopes.InstanceVariables);
            view.AppendCall(instanceScopes.ScopeBlock, subsitutedCall);

            _canCommit = !view.IsAborted;
        }

        protected override bool commit(ExecutionView view)
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
