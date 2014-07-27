using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MEFEditor.Analyzing.Editing.Transformations
{
    public class ActionTransformation:Transformation
    {
        private readonly Action _action;

        public ActionTransformation(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            _action = action;
        }

        protected override void apply()
        {
            _action();
        }
    }
}
