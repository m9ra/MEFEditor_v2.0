using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MEFEditor.Analyzing.Execution;

namespace MEFEditor.Analyzing.Editing.Transformations
{
    class InstanceScope
    {
        /// <summary>
        /// Instance which scope is belonging to
        /// </summary>
        public readonly Instance ScopedInstance;

        /// <summary>
        /// Variable holding <see cref="ScopedInstance"/>
        /// </summary>
        public readonly VariableName Variable;

        /// <summary>
        /// Scope start
        /// </summary>
        public readonly ExecutedBlock Start;

        /// <summary>
        /// Scope end
        /// </summary>
        public readonly ExecutedBlock End;

        /// <summary>
        /// Initialize new <see cref="InstanceScope"/> object
        /// </summary>
        /// <param name="variable">Variable holding <see cref="ScopedInstance"/></param>
        /// <param name="scopedInstance">Instance which scope is belonging to</param>
        /// <param name="start">Scope start</param>
        /// <param name="end">Scope end</param>
        public InstanceScope(VariableName variable, Instance scopedInstance, ExecutedBlock start, ExecutedBlock end)
        {
            Variable = variable;
            ScopedInstance = scopedInstance;
            Start = start;
            End = end;

            if (Variable == null)
                throw new ArgumentNullException("variable");

            if (ScopedInstance == null)
                throw new ArgumentNullException("scopedInstance");

            if (Start == null)
                throw new ArgumentNullException("start");

            if (End == null)
                throw new ArgumentNullException("end");
        }
    }

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
}
