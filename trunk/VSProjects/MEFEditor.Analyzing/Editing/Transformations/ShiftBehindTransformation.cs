using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing.Execution;

namespace MEFEditor.Analyzing.Editing.Transformations
{
    class ShiftBehindTransformation : Transformation
    {
        /// <summary>
        /// Block that will be correctly shifted so it is behind given target
        /// </summary>
        private readonly ExecutedBlock _shifted;

        /// <summary>
        /// Target that needs shifted block to be behind the target
        /// </summary>
        private readonly ExecutedBlock _target;

        internal ShiftBehindTransformation(ExecutedBlock shifted, ExecutedBlock target)
        {
            _shifted = shifted;
            _target = target;
        }

        protected override void apply()
        {
            shiftBehind(_shifted, _target, View);
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


        public static bool Shift(ExecutedBlock toShift, ExecutedBlock target, ExecutionView view)
        {
            var shiftTransformation = new ShiftBehindTransformation(toShift, target);

            view.Apply(shiftTransformation);

            return !view.IsAborted;
        }
    }
}
