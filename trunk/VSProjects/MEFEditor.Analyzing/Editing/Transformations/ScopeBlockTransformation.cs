using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing.Execution;

namespace MEFEditor.Analyzing.Editing.Transformations
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
            if (ScopeVariable == null && !View.IsAborted)
                View.Abort("Cannot get scope variable for instance " + _instance.ID);
        }

        private VariableName instanceScopes(ExecutedBlock scopeBlock, Instance instance)
        {
            //we need to find last call because of keeping semantic of instance
            var lastCall = View.LastCallBlock(instance);
            var lastCallSameLevel = View.GetBlockInLevelOf(scopeBlock.Call, lastCall);

            //detect if it is possible to get valid scope with correct semantic

            if (lastCall == scopeBlock)
                //scope cannot be shifted behind last call
                return null;

            if (lastCall != null)
            {
                if (View.IsBefore(lastCall, scopeBlock))
                {
                    //there is no limit caused by lastCall, 
                    //because lastCall cannot be shifted behind instances scope end
                    //it is automatical upper limit                    
                }
                else
                {
                    if (lastCallSameLevel == null)
                        //last call is in another call tree and after scopeBlock - it cannot be shifted
                        return null;

                    if (!ShiftBehindTransformation.Shift(scopeBlock, lastCallSameLevel, View))
                        //scope block cannot be shifted behind last call
                        return null;
                }
            }

            //find scope that is already opened before scopeBlock
            var scopeVariable = findPreviousValidScope(scopeBlock, instance);
            if (scopeVariable != null)
                //we have find scope variable before scopeBlock
                return scopeVariable;

            //find scope that will be opened afte scope block
            return findNextValidScope(scopeBlock, instance);
        }

        private VariableName findNextValidScope(ExecutedBlock scopeBlock, Instance instance)
        {
            //find scope start
            var scopeStartBlock = View.NextBlock(scopeBlock);
            while (scopeStartBlock != null)
            {
                var starts = scopeStartBlock.ScopeStarts(instance);
                if (starts.Any())
                {
                    if (ShiftBehindTransformation.Shift(scopeBlock, scopeStartBlock, View))
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

        private VariableName findPreviousValidScope(ExecutedBlock scopeBlock, Instance instance)
        {
            //find variable with valid scope
            var block = View.PreviousBlock(scopeBlock);
            var scopeEnds = new HashSet<VariableName>();
            ExecutedBlock firstScopeEnd = null;
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

                if (firstScopeEnd == null && scopeEnds.Count > 0)
                {
                    //if there is no valid variable scope with wanted instance,
                    //then we can try to shift first scope end founded
                    firstScopeEnd = block;
                }

                //shift to next block
                block = View.PreviousBlock(block);
            }

            if (firstScopeEnd != null)
            {
                if (ShiftBehindTransformation.Shift(firstScopeEnd, scopeBlock, View))
                {
                    //scope end was shifted
                    return firstScopeEnd.ScopeEnds(instance).First();
                }
            }
            return null;
        }
    }
}
