using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing.Editing.Transformations
{
    public class AddCallTransformation : Transformation
    {
        /// <summary>
        /// Call provider that will be asked for call definition        
        /// </summary>
        private readonly CallProvider _provider;

        public AddCallTransformation(CallProvider provider)
        {
            _provider = provider;
        }

        protected override void apply()
        {
            var call = _provider(View);
            if (call == null)
                return;

            var scopeTransform = new CommonScopeTransformation(call.Instances);
            View.Apply(scopeTransform);

            if (View.IsAborted)
            {
                //scope transform failed
                return;
            }

            var instanceScopes = scopeTransform.InstanceScopes;
            var subsitutedCall = call.Substitute(instanceScopes.InstanceVariables);
            View.AppendCall(instanceScopes.ScopeBlock, subsitutedCall);
        }
    }
}
