using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Utilities;

using Analyzing.Execution;

namespace Analyzing.Editing.Transformations
{

    internal class InstanceScopes
    {
        /// <summary>
        /// Variables for instances that has common scope
        /// </summary>
        internal readonly Dictionary<Instance, VariableName> InstanceVariables;

        /// <summary>
        /// Scope where variable names are valid
        /// </summary>
        internal readonly ExecutedBlock ScopeBlock;

        public InstanceScopes(Dictionary<Instance, VariableName> variables, ExecutedBlock scopeBlock)
        {
            InstanceVariables = variables;
            ScopeBlock = scopeBlock;
        }
    }

    class ScopeFrame
    {
        private readonly HashSet<Instance> _trackedInstances;

        private MultiDictionary<Instance, VariableName> _activeScopes = new MultiDictionary<Instance, VariableName>();

        private ExecutedBlock _firstScopeEnd;

        private ExecutedBlock _lastScopeStart;

        internal InstanceScopes Scopes { get; private set; }

        internal ScopeFrame(IEnumerable<Instance> trackedInstances)
        {
            _trackedInstances = new HashSet<Instance>(trackedInstances);
        }

        internal void InsertNext(ExecutedBlock block)
        {
            Scopes = null;

            foreach (var tracked in _trackedInstances)
            {
                var scopeStarts = block.ScopeStarts(tracked);
                if (scopeStarts.Any())
                {
                    _activeScopes.Add(tracked, block.ScopeStarts(tracked));
                }

                var scopeEnds = block.ScopeEnds(tracked);
                if (scopeEnds.Any())
                {
                    throw new NotImplementedException();
                }
            }

            var scopes = new Dictionary<Instance, VariableName>();
            foreach (var tracked in _trackedInstances)
            {
                var instanceScopes = _activeScopes.Get(tracked);
                if (!instanceScopes.Any())
                {
                    return;
                }

                scopes.Add(tracked, instanceScopes.First());
            }

            Scopes = new InstanceScopes(scopes, block);
        }
    }
}
