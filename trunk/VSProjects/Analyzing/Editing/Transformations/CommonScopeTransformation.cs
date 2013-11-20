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
        /// <summary>
        /// Instances that common scope is needed
        /// </summary>
        private readonly Instance[] _instances;

        /// <summary>
        /// Result of apply if transformation is successfull, null otherwise
        /// <remarks>Is set on every apply call</remarks>
        /// </summary>
        internal InstanceScopes InstanceScopes;

        internal CommonScopeTransformation(IEnumerable<Instance> instances)
        {
            _instances = instances.ToArray();
        }


        protected override void apply()
        {
            InstanceScopes = getScopes(noShift, _instances);
            if (InstanceScopes == null)
                InstanceScopes = getScopes(tryShift, _instances);
        }

        private InstanceScopes getScopes(ShiftBehind shiftBehind, IEnumerable<Instance> instances)
        {
            var frame = new ScopeFrame(View, shiftBehind, instances);

            //try find common scopes without transforming
            var block = View.EntryBlock;
            while (block != null)
            {
                frame.InsertNext(block);
                block = View.NextBlock(block);

                if (frame.Scopes != null)
                    return frame.Scopes;
            }

            return null;
        }

        private bool noShift(IEnumerable<ExecutedBlock> shiftedBlocks, ExecutedBlock block)
        {
            return false;
        }

        private bool tryShift(IEnumerable<ExecutedBlock> shiftedBlocks, ExecutedBlock block)
        {
            var viewCopy = View.Clone();
            var transforms = new List<ShiftBehindTransformation>();

            foreach (var shifted in shiftedBlocks)
            {
                var shift = new ShiftBehindTransformation(shifted, block);
                viewCopy.Apply(shift);

                if (viewCopy.IsAborted)
                    return false;

                transforms.Add(shift);
            }

            foreach (var transform in transforms)
            {
                View.Apply(transform);
            }

            return true;
        }
    }
}
