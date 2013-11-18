using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution;

namespace Analyzing.Editing.Transformations
{

    class CommonScopeTransformation : Transformation
    {
        internal InstanceScopes InstanceScopes;

        private readonly Instance[] _instances;

        private ExecutionView _view;

        internal CommonScopeTransformation(IEnumerable<Instance> instances)
        {
            _instances = instances.ToArray();
        }


        protected override void apply(ExecutionView services)
        {
            _view = services;

            InstanceScopes = getScopes(_instances);
        }

        protected override bool commit(ExecutionView view)
        {
            return InstanceScopes != null;
        }

        private InstanceScopes getScopes(IEnumerable<Instance> instances)
        {
            var frame = new ScopeFrame(instances);

            //try find common scopes without transforming
            var block = _view.EntryBlock;
            while (block != null)
            {
                frame.InsertNext(block);
                block =_view.NextBlock(block);

                if (frame.Scopes != null)
                    return frame.Scopes;
            }

            return null;
        }
    }
}
