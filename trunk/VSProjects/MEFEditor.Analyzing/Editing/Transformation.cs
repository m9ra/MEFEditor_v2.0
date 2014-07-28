using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing.Editing
{
    /// <summary>
    /// Abstract transformation that can modify <see cref="ExecutionView"/> and provide its edits.
    /// Multiple transformation can be composed together, also single transformation can be used in 
    /// multiple <see cref="ExecutionView"/>.
    /// </summary>
    public abstract class Transformation
    {
        /// <summary>
        /// View available in apply call. The view should
        /// be transformed by current transformation.
        /// <remarks>Is set on every apply call</remarks>.
        /// </summary>
        /// <value>The view.</value>
        protected ExecutionView View { get; private set; }

        /// <summary>
        /// Apply transformation on view stored in View property.
        /// </summary>
        protected abstract void apply();

        /// <summary>
        /// Applies transformation at the given view.
        /// </summary>
        /// <param name="view">The view to be transformed.</param>
        internal void Apply(ExecutionView view)
        {
            View = view;
            try
            {
                apply();
            }
            finally
            {
                View = null;
            }
        }
    }
}
