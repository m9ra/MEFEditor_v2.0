using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;

using Drawing.Behaviours;


namespace Drawing
{
    public abstract class DiagramCanvasBase : Panel
    {
        internal static readonly DropStrategyBase PreviewDropStrategy = new PreviewDropStrategy();

        internal static readonly DropStrategyBase DropStrategy = new DropStrategy();

        internal DiagramItem OwnerItem { get; private set; }

        internal DiagramContext DiagramContext { get; private set; }


        protected DiagramCanvasBase()
        {
            AllowDrop = true;
        }

        #region Position property

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.RegisterAttached("Position", typeof(Point),
            typeof(DiagramCanvasBase), new FrameworkPropertyMetadata(new Point(-1, -1),
            FrameworkPropertyMetadataOptions.AffectsParentArrange));

        public static void SetPosition(UIElement element, Point position)
        {
            element.SetValue(PositionProperty, position);
        }

        public static Point GetPosition(UIElement element)
        {
            return (Point)element.GetValue(PositionProperty);
        }


        #endregion

        internal Point GlobalPosition
        {
            get
            {
                var isRootCanvas = OwnerItem == null;

                if (isRootCanvas)
                {
                    return new Point();
                }
                else
                {
                    var parentGlobal = OwnerItem.GlobalPosition;
                    var parentOffset = OwnerItem.TranslatePoint(new Point(0, 0), this);

                    parentGlobal.X -= parentOffset.X;
                    parentGlobal.Y -= parentOffset.Y;

                    return parentGlobal;
                }
            }
        }


        internal void SetOwner(DiagramItem owner)
        {
            OwnerItem = owner;
            SetContext(owner.DiagramContext);
            Children.Clear();
        }

        internal void SetContext(DiagramContext context)
        {
            DiagramContext = context;
        }

        internal void AddJoin(JoinDrawing join)
        {
            Children.Add(join);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (DiagramContext != null)
            {
                DiagramContext.Provider.Engine.ArrangeChildren(OwnerItem, this);
            }

            foreach (FrameworkElement child in Children)
            {
                var position = GetPosition(child);

                child.Arrange(new Rect(position, child.DesiredSize));

                var item = child as DiagramItem;
                if (item == null)
                    continue;

                //item.RefreshGlobal();
            }
            return arrangeSize;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var totHeight = 0.0;
            var totWidth = 0.0;

            foreach (UIElement child in Children)
            {
                //no borders on child size
                child.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));

                var measured = child.DesiredSize;
                totHeight += measured.Height;
                totWidth += measured.Width;
            }

            var scale = 1.2;
            var reqHeight = Math.Max(totHeight * scale, MinHeight);
            var reqWidth = Math.Max(totWidth * scale, MinWidth);

            //canvas doesn't need no place itself
            return new Size(reqWidth, reqHeight);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            PreviewDropStrategy.OnDrop(this, e);
        }

        protected override void OnDrop(DragEventArgs e)
        {
            DropStrategy.OnDrop(this, e);
            e.Handled = true;
        }


    }
}
