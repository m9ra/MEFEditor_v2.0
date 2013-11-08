using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Editing
{
    public abstract class Transformation
    {
        protected abstract void apply(TransformationServices services);

        protected abstract bool commit();

        public void Apply(TransformationServices services)
        {
            apply(services);
        }

        internal bool Commit()
        {
            return commit();
        }

        public virtual void Abort()
        {
            //by default there is nothing to do
        }
    }
}
