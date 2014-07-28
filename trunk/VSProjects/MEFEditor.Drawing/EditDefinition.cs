using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Drawing
{
    /// <summary>
    /// Action that will edit given view
    /// </summary>
    /// <param name="view">View that will be edited</param>
    /// <returns>Edited view.</returns>
    public delegate EditViewBase EditAction(EditViewBase view);


    /// <summary>
    /// Determine that corresponding edit is active and should
    /// be displayed to the user.
    /// </summary>
    /// <param name="view">View that will be edited</param>
    /// <returns><c>true</c> if edit is active, <c>false</c> otherwise.</returns>
    public delegate bool IsEditActive(EditViewBase view);

    /// <summary>
    /// Representation of edit actions that can be displayed to the user.
    /// </summary>
    public class EditDefinition
    {
        /// <summary>
        /// The name of edit.
        /// </summary>
        internal readonly string Name;

        /// <summary>
        /// The edit action.
        /// </summary>
        internal readonly EditAction Action;

        /// <summary>
        /// Determine that edit is active.
        /// </summary>
        internal readonly IsEditActive IsActive;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditDefinition" /> class.
        /// </summary>
        /// <param name="name">The edit name.</param>
        /// <param name="action">The edit action.</param>
        /// <param name="isActive">Predicate that determine where edit is active.</param>
        public EditDefinition(string name, EditAction action, IsEditActive isActive)
        {
            Name = name;
            Action = action;
            IsActive = isActive;
        }

        /// <summary>
        /// Edit given view and commit it.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <returns><c>true</c> if commit was successful, <c>false</c> otherwise.</returns>
        internal bool Commit(EditViewBase view)
        {
            var editedView = Action(view);

            if (editedView.HasError)
                return false;

            return editedView.Commit();
        }
    }
}
