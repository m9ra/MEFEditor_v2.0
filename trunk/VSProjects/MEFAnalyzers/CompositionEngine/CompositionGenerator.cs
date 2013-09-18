using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

namespace MEFAnalyzers.CompositionEngine
{
    /// <summary>
    /// Provides lazy emitting support
    /// </summary>
    /// <param name="e">Emitter pasted when emitting instructions</param>
    delegate void EmitAction(EmitterBase e);

    class CompositionGenerator:GeneratorBase
    {
        private readonly List<EmitAction> _emitActions = new List<EmitAction>();

        internal void EmitAction(EmitAction action)
        {
            _emitActions.Add(action);
        }

        protected override void generate(EmitterBase emitter)
        {
            foreach (var action in _emitActions)
            {
                action(emitter);
            }
        }
    }
}
