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
        internal readonly DiagramItem Item;

        internal EditViewBase EditView;

        Vector _center;
        DiagramCanvas _dragScope;
        AdornerLayer _dragLayer;

        Brush _visualBrush;
        Pen _visualPen;

        Rectangle visChild = new Rectangle();

        StackPanel _visual = new StackPanel();

        private Point _startPosition;

        /// <summary>
        /// Hint displayed when adorner is dragged.
        /// </summary>
        internal string Hint;

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
            Item = item;
            _dragScope = item.DiagramContext.Provider.Output;
            _dragLayer = AdornerLayer.GetAdornerLayer(_dragScope);

            _visualBrush = new VisualBrush(item);
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
            var oldPos = globalPos;

            if (Item.OutOfBounds(ref globalPos))
            {
                if (Item.CanExcludeFromParent)
                {
                    globalPos = oldPos;
                }
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
            drawingContext.DrawRectangle(_visualBrush, _visualPen, new Rect(pos, new Size(Item.ActualWidth, Item.ActualHeight)));



            //TODO optimize, refactor, change,... this is only for visual testing :]

            var typeFace = new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
            var formattedHint = new FormattedText(Hint, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeFace, 12, Brushes.Blue);

            var padding = 5;
            var textPos = new Point(pos.X + _center.X + 3 * padding, pos.Y + _center.Y + 3 * padding);

            var fontRectPos = new Point(textPos.X - padding, textPos.Y - padding);
            var fontRect = new Rect(fontRectPos, new Size(formattedHint.Width + padding * 2, formattedHint.Height + padding * 2));

            var brush = new LinearGradientBrush(Colors.White, Colors.LightGray, 90);
            var pen = new Pen(Brushes.Gray, 1);

            drawingContext.DrawRoundedRectangle(brush, pen, fontRect, 5, 5);
            drawingContext.DrawText(formattedHint, textPos);
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
