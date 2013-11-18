using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Editing
{
    public abstract class Transformation
    {
        protected abstract void apply(ExecutionView view);

        protected abstract bool commit(ExecutionView view);

        public void Apply(ExecutionView view)
        {
            apply(view);
        }

        internal bool Commit(ExecutionView view)
        {
            return commit(view);
        }
    }
}
