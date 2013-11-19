﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution;

namespace Analyzing.Editing.Transformations
{
    class CommonScopeTransformation : Transformation
    {
        private readonly Instance[] _instances;

        private ExecutionView _view;

        internal InstanceScopes InstanceScopes;

        internal CommonScopeTransformation(IEnumerable<Instance> instances)
        {
            _instances = instances.ToArray();
        }


        protected override void apply(ExecutionView services)
        {
            _view = services;

            InstanceScopes = getScopes(noShift,_instances);
            if (InstanceScopes == null)
                InstanceScopes = getScopes(tryShift, _instances);
        }

        protected override bool commit(ExecutionView view)
        {
            return InstanceScopes != null;
        }

        private InstanceScopes getScopes(ShiftBehind shiftBehind, IEnumerable<Instance> instances)
        {
            var frame = new ScopeFrame(_view, shiftBehind, instances);

            //try find common scopes without transforming
            var block = _view.EntryBlock;
            while (block != null)
            {
                frame.InsertNext(block);
                block = _view.NextBlock(block);

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
            var viewCopy = _view.Clone();
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
                _view.Apply(transform);
            }

            return true;
        }
    }
}
