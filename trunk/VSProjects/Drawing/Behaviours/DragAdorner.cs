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

using System.Globalization;

namespace Drawing.Behaviours
{
    /// <summary>
    /// Representation of drag adorner, inspirated by http://jaimersamples.members.winisp.net/samples/dragdrop/samples.zip   
    /// </summary>
    class DragAdorner : Adorner
    {
        private readonly Brush _visualBrush = null;

        private readonly Pen _visualPen = null;

        private readonly MouseHint _hint;

        private Vector _center;

        private DiagramCanvas _dragScope;

        private AdornerLayer _dragLayer;
        
        private Point _startPosition;

        internal readonly DiagramItem Item;

        internal EditViewBase EditView;

        /// <summary>
        /// Hint displayed when adorner is dragged.
        /// </summary>
        internal string Hint;

        /// <summary>
        /// Position of adorner relative to workspace
        /// </summary>
        public Point GlobalPosition { get; private set; }



        /// <summary>
        /// Create adorner which will show dragged object.   
        /// </summary>
        /// <param name="item">Dragged object.</param>
        /// <param name="dragStart">Drag start in mouse relative coordinates!!</param>
        public DragAdorner(DiagramItem item, Point dragStart)
            : base(item)
        {
            Item = item;

            _dragScope = item.DiagramContext.Provider.Output;

            _dragLayer = AdornerLayer.GetAdornerLayer(_dragScope);
            _dragLayer.Add(this);

            _hint = new MouseHint(_dragScope);


            _visualBrush = new VisualBrush(item);
            _visualBrush.Opacity = 0.5;


            _startPosition = item.GlobalPosition;
            GlobalPosition = _startPosition;

            _center = getScaledPosition(dragStart) - _startPosition;



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
            _hint.UpdateCursor(globalPos);
            _hint.HintText = Hint;

            globalPos = getScaledPosition(globalPos);

            globalPos.X -= _center.X;
            globalPos.Y -= _center.Y;
            var oldPos = globalPos;

            if (Item.OutOfBounds(ref globalPos))
            {
                if (Item.CanExcludeFromParent)
                {
                    //don't need to be bounded
                    globalPos = oldPos;
                }
            }

            GlobalPosition = globalPos;
            _dragLayer.Update(this.AdornedElement);
        }

        private Point getScaledPosition(Point rawPoint)
        {
            var scaleFactor = _dragScope.Zoom;

            return new Point(rawPoint.X / scaleFactor, rawPoint.Y / scaleFactor);
        }
        
        internal void DragEnd()
        {
            _dragScope.PreviewDragOver -= _updatePosition;
            _dragLayer.Remove(this);
            _hint.HintEnd();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var offset = GlobalPosition - _startPosition;

            var pos = new Point(offset.X, offset.Y);
            drawingContext.DrawRectangle(_visualBrush, _visualPen, new Rect(pos, new Size(Item.ActualWidth, Item.ActualHeight)));
        }

        protected override Size MeasureOverride(Size finalSize)
        {
            return Item.DesiredSize;
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            return finalSize;
        }
    }
}
