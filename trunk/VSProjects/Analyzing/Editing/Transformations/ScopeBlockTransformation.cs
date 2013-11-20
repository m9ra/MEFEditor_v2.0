using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution;

namespace Analyzing.Editing
{
    class ScopeBlockTransformation : Transformation
    {
        /// <summary>
        /// Block where instance scope has to be valid
        /// </summary>
        private readonly ExecutedBlock _scopeBlock;

        /// <summary>
        /// Instance that scope is needed for block
        /// </summary>
        private readonly Instance _instance;

        /// <summary>
        /// Variable that is result of transformation application
        /// <remarks>Is set on every apply call</remarks>
        /// </summary>
        internal VariableName ScopeVariable { get; private set; }


        internal ScopeBlockTransformation(ExecutedBlock scopeBlock, Instance instance)
        {
            _instance = instance;
            _scopeBlock = scopeBlock;
        }

        protected override void apply()
        {
            ScopeVariable = instanceScopes(_scopeBlock, _instance);
        }

        private VariableName instanceScopes(ExecutedBlock scopeBlock, Instance instance)
        {
            //find variable with valid scope
            var block = View.PreviousBlock(scopeBlock);
            var scopeEnds = new HashSet<VariableName>();
            ExecutedBlock _firstScopeEnd = null;
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
                block = View.PreviousBlock(block);
            }

            if (_firstScopeEnd != null)
            {
                if (shiftBehind(_firstScopeEnd, scopeBlock, View))
                {
                    //scope end was shifted
                    return _firstScopeEnd.ScopeEnds(instance).First();
                }
            }

            //find scope start
            var scopeStartBlock = View.NextBlock(scopeBlock);
            while (scopeStartBlock != null)
            {
                var starts = scopeStartBlock.ScopeStarts(instance);
                if (starts.Any())
                {
                    if (shiftBehind(scopeBlock, scopeStartBlock, View))
                    {
                        return starts.First();
                    }
                    break;
                }
                scopeStartBlock = View.NextBlock(scopeStartBlock);
            }

            //cannot find valid scope
            return null;
        }

        /// <summary>
        /// Shift given shiftedBlock behind target, if possible
        /// </summary>
        /// <param name="shiftedBlock"></param>
        /// <param name="target"></param>
        /// <param name="view"></param>
        private bool shiftBehind(ExecutedBlock shiftedBlock, ExecutedBlock target, ExecutionView view)
        {
            //cumulative list of blocks that has to be shifted
            //It has reverse ordering of transformations that will be generated            
            var shiftedBlocks = new List<ExecutedBlock>();
            shiftedBlocks.Add(shiftedBlock);

            var borderInstances = new HashSet<Instance>();
            borderInstances.UnionWith(view.AffectedInstances(shiftedBlock));

            //find all colliding blocks, so we can move them with shifted block if possible
            var currentBlock = shiftedBlock;
            while (currentBlock != target)
            {
                currentBlock = view.NextBlock(currentBlock);

                if (!canCross(currentBlock, borderInstances, view))
                {
                    //this block cannot be crossed
                    borderInstances.UnionWith(view.AffectedInstances(currentBlock));
                    shiftedBlocks.Add(currentBlock);
                }
            }

            //shifting is not possible, due to collisions between blocks
            if (!canCross(target, borderInstances, view))
            {
                return false;
            }

            shiftedBlocks.Reverse();
            foreach (var block in shiftedBlocks)
            {
                view.ShiftBehind(block, target);
            }

            return true;
        }


        private bool canCross(ExecutedBlock shiftedBlock, HashSet<Instance> borderInstances, ExecutionView view)
        {
            foreach (var instance in view.AffectedInstances(shiftedBlock))
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
