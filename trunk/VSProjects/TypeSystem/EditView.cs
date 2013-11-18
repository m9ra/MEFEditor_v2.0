using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Drawing;
using Analyzing.Editing;

namespace TypeSystem
{
    public class EditView : EditViewBase
    {
        private readonly ExecutionView _data;

        public override string Error { get { return _data.AbortMessage; } }

        public EditView(ExecutionView data)
        {
            _data = data.Clone();
        }

        protected override void commit()
        {
            if (_data.IsAborted)
                return;

            _data.Commit();
        }

        public EditView Clone()
        {
            return new EditView(_data);
        }

        internal EditViewBase Apply(Transformation transformation)
        {
            var clonned = Clone();
            clonned._data.Apply(transformation);

            return clonned;
        }
    }
}
