using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Editing
{
    public abstract class Transformation
    {
        /// <summary>
        /// View available in apply call
        /// <remarks>Is set on every apply call</remarks>
        /// </summary>
        protected ExecutionView View { get; private set; }

        /// <summary>
        /// Apply transformation on view stored in View property
        /// </summary>
        protected abstract void apply();

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
