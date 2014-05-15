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

    delegate bool BehindShifter(IEnumerable<ExecutedBlock> shiftedBlocks, ExecutedBlock block);

    class ScopeFrame
    {
        private readonly HashSet<Instance> _trackedInstances;

        private readonly ExecutionView _view;

        private readonly BehindShifter _shiftBehind;

        private MultiDictionary<Instance, VariableName> _activeScopes = new MultiDictionary<Instance, VariableName>();

        private Dictionary<Instance, ExecutedBlock> _lastScopeEnds = new Dictionary<Instance, ExecutedBlock>();

        internal InstanceScopes Scopes { get; private set; }

        internal ScopeFrame(ExecutionView view, BehindShifter shiftBehind, IEnumerable<Instance> trackedInstances)
        {
            _view = view;
            _shiftBehind = shiftBehind;
            _trackedInstances = new HashSet<Instance>(trackedInstances);
        }

        internal void InsertNext(ExecutedBlock block)
        {
            Scopes = null;

            foreach (var tracked in _trackedInstances)
            {
                var scopeStarts = _view.ScopeStarts(block, tracked);
                if (scopeStarts.Any())
                {
                    _activeScopes.Add(tracked, block.ScopeStarts(tracked));
                }

                var scopeEnds = _view.ScopeEnds(block, tracked);
                foreach (var scopeEnd in scopeEnds)
                {
                    _activeScopes.Remove(tracked, scopeEnd);
                    _lastScopeEnds[tracked] = block;
                }
            }

            var scopes = new Dictionary<Instance, VariableName>();
            var endScopes = new List<ExecutedBlock>();
            foreach (var tracked in _trackedInstances)
            {
                var instanceScopes = _activeScopes.Get(tracked);
                if (!instanceScopes.Any())
                {
                    //doesnt have any active scopes - try to shift
                    if (_lastScopeEnds.ContainsKey(tracked))
                    {
                        var scopeEnd = _lastScopeEnds[tracked];
                        endScopes.Add(scopeEnd);
                        scopes.Add(tracked, _view.ScopeEnds(scopeEnd, tracked).First());
                        continue;
                    }
                    else
                    {
                        //definetly missing scope
                        return;
                    }
                }

                scopes.Add(tracked, instanceScopes.First());
            }

            if (endScopes.Count > 0)
            {
                if (!_shiftBehind(endScopes, block))
                    return;
            }

            Scopes = new InstanceScopes(scopes, block);
        }
    }
}
