using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution;

namespace Analyzing.Editing.Transformations
{
    /// <summary>
    /// Handler used for determining if given transformations will be accepted or not
    /// </summary>
    /// <param name="transformations">Transformations required for desired operation</param>
    /// <returns><c>truee</c> if transformations were accepted, <c>false</c> otherwise</returns>
    delegate bool TransformationAccepter(IEnumerable<Transformation> transformations);

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
            InstanceScopes = getScopes(_instances);

            if (InstanceScopes == null)
                View.Abort("Can't get instances to same scope");
        }

        private InstanceScopes getScopes(IEnumerable<Instance> instances)
        {
            var monitor = new ScopeMonitor(instances, View);

            var lastCalls = getLastCalls(instances);
            var lastCall = View.LatestBlock(lastCalls);

            var scopes = getScopes(lastCall, monitor.CreateStepper(), noTransformations);
            if (scopes != null)
                //there are common scopes without need for shifting
                return scopes;

            return getScopes(lastCall, monitor.CreateStepper(), applyTransformations);
        }

        private InstanceScopes getScopes(ExecutedBlock lastCall, ScopeStepper scopeStepper, TransformationAccepter transformationAccepter)
        {
            IEnumerable<InstanceScope> scopes;
            while ((scopes = scopeStepper.Step(View)) != null)
            {
                var scopeEnds = from scope in scopes select scope.End;
                var scopeStarts = from scope in scopes select scope.Start;

                var latestStart = View.LatestBlock(scopeStarts);
                var earliestEnd = View.EarliestBlock(scopeEnds);

                //end shouldnt been specified - limiting is last call
                var scopeBlock = earliestEnd == null ? lastCall : earliestEnd;
                //if there is no last call, we can use latest start
                scopeBlock = scopeBlock == null ? latestStart : scopeBlock;

                var transformations = getScopeTransformations(lastCall, scopeBlock, scopes);

                if (transformations == null)
                    continue;

                //we have common scope after making all transformations
                if (transformationAccepter(transformations))
                {
                    return createInstanceScopes(scopes, scopeBlock);
                }
            }

            //cannot shift to common scope
            return null;
        }

        private IEnumerable<Transformation> getScopeTransformations(ExecutedBlock lastCall, ExecutedBlock latestStart, IEnumerable<InstanceScope> scopes)
        {
            var transformations = new List<Transformation>();

            var testView = View.Clone();

            //we need to have scope after last call
            if (!ensurePosition(lastCall, latestStart, testView, transformations))
                return null;

            //try to get all scope ends behind latestStart
            foreach (var scope in scopes)
            {
                //we need to have all scope ends behind latestStart
                if (!ensurePosition(scope.End, latestStart, testView, transformations))
                {
                    return null;
                }
            }

            return transformations;
        }

        private IEnumerable<ExecutedBlock> getLastCalls(IEnumerable<Instance> instances)
        {
            var lastCalls = new HashSet<ExecutedBlock>();

            foreach (var instance in instances)
            {
                var lastCall = View.LastCallBlock(instance);
                if (lastCall == null)
                    //no call on instance
                    continue;

                lastCalls.Add(lastCall);
            }
            return lastCalls;
        }

        private InstanceScopes createInstanceScopes(IEnumerable<InstanceScope> scopes, ExecutedBlock scopeBlock)
        {
            var scopeIndex = new Dictionary<Instance, VariableName>();
            foreach (var scope in scopes)
            {
                scopeIndex.Add(scope.ScopedInstance, scope.Variable);
            }

            return new InstanceScopes(scopeIndex, scopeBlock);
        }

        private bool ensurePosition(ExecutedBlock beforeBlock, ExecutedBlock afterBlock, ExecutionView view, List<Transformation> transformations)
        {
            if (view.IsBefore(beforeBlock, afterBlock))
                //no transformation is required
                return true;

            //we need to get after block behind before block
            var shiftTransform = new ShiftBehindTransformation(afterBlock, beforeBlock);
            transformations.Add(shiftTransform);
            view.Apply(shiftTransform);

            return !view.IsAborted;
        }


        private bool noTransformations(IEnumerable<Transformation> transformations)
        {
            return !transformations.Any();
        }


        private bool applyTransformations(IEnumerable<Transformation> transformations)
        {
            foreach (var transformation in transformations)
            {
                View.Apply(transformation);
            }

            return true;
        }

    }
}
