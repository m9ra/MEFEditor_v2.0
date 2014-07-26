using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Drawing;
using Analyzing.Editing;

namespace TypeSystem
{
    public class EditView : EditViewBase
    {
        private readonly ExecutionView _data;

        public override string Error { get { return _data.AbortMessage; } }

        public EditView(ExecutionView data)
            : this(data, true)
        {
        }

        private EditView(ExecutionView data, bool copy)
        {
            _data = copy ? data.Clone() : data;
        }

        public static EditViewBase Wrap(ExecutionView v)
        {
            return new EditView(v,false);
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

        public ExecutionView CopyView()
        {
            return _data.Clone();
        }

        internal EditViewBase Apply(Transformation transformation)
        {
            var clonned = Clone();
            clonned._data.Apply(transformation);

            return clonned;
        }

    }
}
