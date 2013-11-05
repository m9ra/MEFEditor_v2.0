using System;
using System.Collections.Generic;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

namespace Drawing.Behaviours
{
    /// <summary>
    /// Representation of drag adorner, inspirated by http://jaimersamples.members.winisp.net/samples/dragdrop/samples.zip   
    /// </summary>
    class DragAdorner : Adorner
    {
        DiagramItem _item;
        Vector _center;
        DiagramCanvas _dragScope;
        AdornerLayer _dragLayer;

        Brush _visualBrush;
        Pen _visualPen;

        Rectangle visChild = new Rectangle();

        private Point _startPosition;

        /// <summary>
        /// Number of Visual children.
        /// </summary>
        protected override int VisualChildrenCount { get { return 1; } }

        /// <summary>
        /// Position of adorner relative to workspace
        /// </summary>
        public Point GlobalPosition { get; private set; }



        /// <summary>
        /// Create adorner which will show dragged object.   
        /// </summary>
        /// <param name="item">Dragged object.</param>
        /// <param name="dragScope">Bounds where can be thumb draged.</param>
        /// <param name="center">Point where the drag started relative to adornElement.</param>
        public DragAdorner(DiagramItem item, Point dragStart)
            : base(item)
        {
            _item = item;
            _dragScope = item.DiagramContext.Provider.Output;
            _dragLayer = AdornerLayer.GetAdornerLayer(_dragScope);

            _visualBrush = new VisualBrush(_item);
            _visualBrush.Opacity = 0.5;

            _startPosition = item.GlobalPosition;
            GlobalPosition = _startPosition;

            _center = dragStart - _startPosition;

            IsHitTestVisible = false;
            _dragScope.PreviewDragOver += _updatePosition;
        }


        /// <summary>
        /// Cause that adorner position is updated according to drag events.
        /// </summary>
        /// <param name="e">Drag events.</param>
        private void _updatePosition(object sender, DragEventArgs e)
        {
            var globalPos = e.GetPosition(_dragScope);
            globalPos.X -= _center.X;
            globalPos.Y -= _center.Y;


            if (_item.OutOfBounds(ref globalPos))
            {
                throw new NotImplementedException("Check if item can be excluded from parent");
            }


            GlobalPosition = globalPos;

            _dragLayer.Update(this.AdornedElement);
        }

        protected override Visual GetVisualChild(int index)
        {
            return visChild;
        }

        internal void DragStart()
        {
            _dragLayer.Add(this);
        }

        internal void DragEnd()
        {
            _dragScope.PreviewDragOver -= _updatePosition;
            _dragLayer.Remove(this);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var offset = GlobalPosition - _startPosition;

            var pos = new Point(offset.X, offset.Y);
            drawingContext.DrawRectangle(_visualBrush, _visualPen, new Rect(pos, new Size(_item.ActualWidth, _item.ActualHeight)));
        }

        protected override Size MeasureOverride(Size finalSize)
        {
            return _item.DesiredSize;
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            return finalSize;
        }
    }
}
