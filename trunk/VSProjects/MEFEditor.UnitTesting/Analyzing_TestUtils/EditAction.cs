using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;

namespace MEFEditor.UnitTesting.Analyzing_TestUtils
{
    /// <summary>
    /// Testing representation of edit action, that simulates
    /// edit from user IO.
    /// </summary>
    public class EditAction
    {
        /// <summary>
        /// The variable where is located edited <see cref="Instance"/>
        /// </summary>
        public readonly VariableName Variable;

        /// <summary>
        /// The name of represented edit.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Determine that edit will remove <see cref="Instance"/>.
        /// </summary>
        public readonly bool IsRemoveAction;

        /// <summary>
        /// If name is null, it is an remove action.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="name">The name.</param>
        private EditAction(VariableName variable, string name)
        {
            Variable = variable;
            Name = name;
            IsRemoveAction = name == null;
        }

        /// <summary>
        /// Creates edit action that edits <see cref="Instance"/> specified by variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="editName">Name of the edit.</param>
        /// <returns>EditAction.</returns>
        /// <exception cref="System.ArgumentNullException">editName</exception>
        internal static EditAction Edit(VariableName variable, string editName)
        {
            if (editName == null)
                throw new ArgumentNullException("editName");

            var action = new EditAction(variable, editName);
            return action;
        }

        /// <summary>
        /// Creates edit action that removes <see cref="Instance"/> specified by variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>EditAction.</returns>
        internal static EditAction Remove(VariableName variable)
        {
            var action = new EditAction(variable, null);
            return action;
        }
    }
}
