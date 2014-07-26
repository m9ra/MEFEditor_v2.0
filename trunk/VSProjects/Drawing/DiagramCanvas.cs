using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Windows;
using System.Windows.Media;

using System.ComponentModel;


namespace Drawing
{
    /// <summary>
    /// <see cref="Canvas"/> specialization for providing services of MEFEditor.Drawing library.
    /// It is root of displayed diagram.
    /// </summary>
    public class DiagramCanvas : DiagramCanvasBase
    {
        /// <summary>
        /// Scale transformation used for zooming
        /// </summary>
        private readonly ScaleTransform _scale = new ScaleTransform();

        /// <summary>
        /// Shift <see cref="Vector"/> used for content shifting
        /// </summary>
        private Vector _shift;

        /// <summary>
        /// Set or get vector that determine shifting of content 
        /// displayed on <see cref="DiagramCanvas"/>
        /// </summary>
        public Vector Shift
        {
            get
            {
                return _shift;
            }

            set
            {
                if (_shift == value)
                    return;

                _shift = value;
                InvalidateArrange();
            }
        }

        /// <summary>
        /// Zoom of displayed content
        /// </summary>
        public double Zoom
        {
            get
            {
                //both scale factors has same value
                return _scale.ScaleX;
            }
            set
            {
                if (_scale.ScaleX == value)
                    return;

                _scale.ScaleX = value;
                _scale.ScaleY = value;

                InvalidateArrange();
            }
        }

        /// <summary>
        /// Clear displayed content
        /// </summary>
        public void Clear()
        {
            Children.Clear();
            ContextMenu = null;
        }

        /// <summary>
        /// Reset content transformations
        /// </summary>
        public void Reset()
        {
            Shift = new Vector(0, 0);
            Zoom = 1;
        }

        /// <inheritdoc />
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            foreach (FrameworkElement child in Children)
            {
                child.RenderTransform = _scale;

                var position = GetPosition(child);
                position = _scale.Transform(position);

                position.X += Shift.X;
                position.Y += Shift.Y;

                child.Arrange(new Rect(position, child.DesiredSize));
            }
            return arrangeSize;
        }

    }
}
