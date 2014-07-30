using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing.Execution.Instructions
{
    /// <summary>
    /// New created <see cref="Instance"/> to variable assign instruction.
    /// </summary>
    class AssignNewObject : AssignBase
    {
        /// <summary>
        /// The target variable.
        /// </summary>
        private readonly VariableName _targetVariable;

        /// <summary>
        /// The object information.
        /// </summary>
        private readonly InstanceInfo _objectInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignNewObject" /> class.
        /// </summary>
        /// <param name="targetVariable">The target variable.</param>
        /// <param name="objectInfo">The object information.</param>
        internal AssignNewObject(VariableName targetVariable, InstanceInfo objectInfo)
        {
            _objectInfo = objectInfo;
            _targetVariable = targetVariable;
        }

        /// <summary>
        /// Executes instruction in given context.
        /// </summary>
        /// <param name="context">Context where instruction is executed.</param>
        public override void Execute(AnalyzingContext context)
        {
            var newInstance = context.Machine.CreateInstance(_objectInfo);
            if (RemoveProvider != null)
                newInstance.HintCreationNavigation(RemoveProvider.GetNavigation());

            newInstance.CreationBlock = context.CurrentCall.CurrentBlock;

            context.SetValue(_targetVariable, newInstance);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("mov_new {0}, {1}", _targetVariable, _objectInfo);
        }
    }
}
