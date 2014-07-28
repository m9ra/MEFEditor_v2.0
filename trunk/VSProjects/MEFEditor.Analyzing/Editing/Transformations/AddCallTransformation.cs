using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing.Editing.Transformations
{
    /// <summary>
    /// Transformation for adding calls into <see cref="ExecutionView"/>.
    /// </summary>
    public class AddCallTransformation : Transformation
    {
        /// <summary>
        /// Call provider that will be asked for call definition.
        /// </summary>
        private readonly CallProvider _provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddCallTransformation"/> class.
        /// </summary>
        /// <param name="provider">Call provider that will be asked for call definition.</param>
        public AddCallTransformation(CallProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Apply transformation on view stored in View property
        /// </summary>
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
