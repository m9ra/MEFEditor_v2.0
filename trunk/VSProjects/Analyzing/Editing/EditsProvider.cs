using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution;


namespace Analyzing.Editing
{
    /// <summary>
    /// Provides value for edit on editedInstance
    /// </summary>
    /// <returns>Value that will be pasted to transformation provider. The transformation provider decide, that it understand given value.</returns>
    public delegate object ValueProvider(TransformationServices services);

    public class EditsProvider<MethodID, InstanceInfo>
    {
        readonly CallTransformProvider _callProvider;

        readonly ExecutedBlock<MethodID, InstanceInfo> _block;

        internal EditsProvider(CallTransformProvider callProvider, ExecutedBlock<MethodID, InstanceInfo> block)
        {
            if (callProvider == null)
            {
                throw new ArgumentNullException("callProvider");
            }
            _callProvider = callProvider;
            _block = block;
        }

        public VariableName GetVariableFor(Instance instance, TransformationServices services)
        {
            //find variable with valid scope
            var block = _block.PreviousBlock;
            var scopeEnds = new HashSet<VariableName>();
            ExecutedBlock<MethodID, InstanceInfo> _firstScopeEnd = null;
            while (block != null)
            {
                scopeEnds.UnionWith(block.ScopeEnds(instance));

                foreach (var start in block.ScopeStarts(instance))
                {
                    if (!scopeEnds.Contains(start))
                    {
                        //we have found variable with valid scope
                        return start;
                    }
                }

                if (_firstScopeEnd == null && scopeEnds.Count > 0)
                {
                    //if there is no valid variable scope with wanted instance,
                    //then we can try to shift first scope end founded
                    _firstScopeEnd = block;
                }

                //shift to next block
                block = block.PreviousBlock;
            }

            if (_firstScopeEnd != null)
            {
                if (shiftBehind(_firstScopeEnd, _block, services))
                {
                    //scope end was shifted
                    return _firstScopeEnd.ScopeEnds(instance).First();
                }
                throw new NotImplementedException();
            }

            //cannot find valid scope
            return null;
        }

        public void AppendArgument(Instance editProvider, string editName, ValueProvider valueProvider)
        {
            var transformation = _callProvider.AppendArgument(valueProvider);
            addEdit(editProvider, editName, transformation);
        }

        public void RemoveArgument(Instance editProvider, int argumentIndex, string editName)
        {
            var transformation = _callProvider.RemoveArgument(argumentIndex);
            addEdit(editProvider, editName, transformation);
        }

        public void ChangeArgument(Instance editProvider, int argumentIndex, string editName, ValueProvider valueProvider)
        {
            var transformation = _callProvider.RewriteArgument(argumentIndex, valueProvider);
            addEdit(editProvider, editName, transformation);
        }

        private void addEdit(Instance editProvider, string editName, Transformation transformation)
        {
            var edit = new Edit(editName, transformation);
            editProvider.AddEdit(edit);
        }

        /// <summary>
        /// Shift given shiftedBlock behind target, if possible
        /// </summary>
        /// <param name="shiftedBlock"></param>
        /// <param name="target"></param>
        /// <param name="services"></param>
        private bool shiftBehind(ExecutedBlock<MethodID, InstanceInfo> shiftedBlock, ExecutedBlock<MethodID, InstanceInfo> target, TransformationServices services)
        {
            //cumulative list of blocks that has to be shifted
            //It has reverse ordering of transformations that will be generated
            //Hint: Shifted block will be added because of border instances
            var shiftedBlocks = new List<ExecutedBlock<MethodID, InstanceInfo>>();
          
            var borderInstances = new HashSet<Instance>();
            borderInstances.UnionWith(shiftedBlock.AffectedInstances);

            //find all colliding blocks, so we can move them with shifted block if possible
            var currentBlock = shiftedBlock;
            while (currentBlock != target)
            {
                if (!canCross(currentBlock, borderInstances))
                {
                    //this block cannot be crossed
                    borderInstances.UnionWith(currentBlock.AffectedInstances);
                    shiftedBlocks.Add(currentBlock);
                }

                currentBlock = currentBlock.NextBlock;
            }

            //shifting is not possible, due to collisions between blocks
            if (!canCross(target, borderInstances))
            {
                return false;
            }
            
            shiftedBlocks.Reverse();
            foreach (var block in shiftedBlocks)
            {
                var shiftTransform = block.Info.ShiftingProvider.ShiftBehind(target.Info.ShiftingProvider);
                services.Apply(shiftTransform);
            }

            return true;
        }


        private bool canCross(ExecutedBlock<MethodID, InstanceInfo> shiftedBlock, HashSet<Instance> borderInstances)
        {
            foreach (var instance in shiftedBlock.AffectedInstances)
            {
                if (borderInstances.Contains(instance))
                {
                    //there is collision with border
                    return false;
                }
            }

            return true;
        }
    }
}
