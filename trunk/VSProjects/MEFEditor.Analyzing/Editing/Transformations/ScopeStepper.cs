using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Utilities;

namespace MEFEditor.Analyzing.Editing.Transformations
{
    class ScopeStepper
    {
        IEnumerable<IEnumerator<InstanceScope>> _enumerators;

        public ScopeStepper(MultiDictionary<Instance, InstanceScope> scopes)
        {
            _enumerators = initializeFrom(scopes);
        }

        /// <summary>
        /// Get sweep line of instance scopes
        /// at end returns <c>null</c>
        /// </summary>
        /// <param name="view">View where scopes are stepped</param>
        /// <returns>Sweep line if there are available scopes, <c>null</c> otherwise</returns>
        public IEnumerable<InstanceScope> Step(ExecutionView view)
        {
            if (_enumerators == null)
                //stepper is already at the end
                return null;

            var result = (from enumerator in _enumerators select enumerator.Current).ToArray();
            var currentEnds = from scope in result select scope.End;

            var earliestEnd = view.EarliestBlock(currentEnds);

            foreach (var enumerator in _enumerators)
            {
                if (enumerator.Current.End == earliestEnd)
                {
                    if (!enumerator.MoveNext())
                    {
                        //there is no more scopes for instance
                        _enumerators = null;
                    }
                    break;
                }
            }

            return result;
        }

        private IEnumerable<IEnumerator<InstanceScope>> initializeFrom(MultiDictionary<Instance, InstanceScope> scopes)
        {
            var scopeEnumerators = new List<IEnumerator<InstanceScope>>();

            foreach (var instance in scopes.Keys)
            {
                var enumerator = scopes.Get(instance).GetEnumerator();
                if (!enumerator.MoveNext())
                    //there is no scope for instance
                    return null;

                scopeEnumerators.Add(enumerator);
            }

            return scopeEnumerators;
        }
    }
}
