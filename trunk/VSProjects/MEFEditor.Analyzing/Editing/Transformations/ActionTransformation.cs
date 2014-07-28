using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MEFEditor.Analyzing.Editing.Transformations
{
    /// <summary>
    /// Transformation that doesn't apply any view edits and only run specified action.
    /// </summary>
    public class ActionTransformation : Transformation
    {
        /// <summary>
        /// The specified action.
        /// </summary>
        private readonly Action _action;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionTransformation"/> class.
        /// </summary>
        /// <param name="action">The action that will be runned on transformation apply.</param>
        /// <exception cref="System.ArgumentNullException">action</exception>
        public ActionTransformation(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            _action = action;
        }

        /// <summary>
        /// Run stored action
        /// </summary>
        protected override void apply()
        {
            _action();
        }
    }
}
