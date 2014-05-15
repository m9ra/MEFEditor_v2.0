using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Analyzing.Execution;

namespace Analyzing.Editing.Transformations
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
        }
    }
}
