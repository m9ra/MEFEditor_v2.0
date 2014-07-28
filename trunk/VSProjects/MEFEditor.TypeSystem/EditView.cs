using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Drawing;
using MEFEditor.Analyzing.Editing;

namespace MEFEditor.TypeSystem
{
    /// <summary>
    /// Wrapper for <see cref="ExecutionView"/> that can be used in <see cref="MEFEditor.Drawing"/> library.
    /// </summary>
    public class EditView : EditViewBase
    {
        /// <summary>
        /// The wrapped view.
        /// </summary>
        private readonly ExecutionView _data;

        /// <summary>
        /// Error occurred in the view during commit or view creation
        /// </summary>
        /// <value>The error.</value>
        public override string Error { get { return _data.AbortMessage; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditView"/> class.
        /// </summary>
        /// <param name="data">The wrapped view.</param>
        public EditView(ExecutionView data)
            : this(data, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditView"/> class.
        /// </summary>
        /// <param name="data">The wrapped view.</param>
        /// <param name="copy">if set to <c>true</c> clone the view.</param>
        private EditView(ExecutionView data, bool copy)
        {
            _data = copy ? data.Clone() : data;
        }

        /// <summary>
        /// Wraps the specified view.
        /// </summary>
        /// <param name="v">The wrapped view.</param>
        /// <returns>Wrapped view.</returns>
        public static EditViewBase Wrap(ExecutionView v)
        {
            return new EditView(v,false);
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>EditView.</returns>
        public EditView Clone()
        {
            return new EditView(_data);
        }

        /// <summary>
        /// Copies the wrapped view.
        /// </summary>
        /// <returns>Copy of wrapped view.</returns>
        public ExecutionView CopyView()
        {
            return _data.Clone();
        }

        /// <summary>
        /// Aborts view with the specified error.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <returns>Aborted view.</returns>
        internal EditView Abort(string error)
        {
            var aborted = new EditView(_data, true);
            if (!aborted.HasError)
                aborted.Abort(error);

            return aborted;
        }
        
        /// <summary>
        /// Applies the specified transformation.
        /// </summary>
        /// <param name="transformation">The transformation.</param>
        /// <returns>EditViewBase.</returns>
        internal EditViewBase Apply(Transformation transformation)
        {
            var clonned = Clone();
            clonned._data.Apply(transformation);

            return clonned;
        }

        /// <summary>
        /// Commit current view.
        /// </summary>        
        protected override void commit()
        {
            if (_data.IsAborted)
                return;

            _data.Commit();
        }

    }
}
