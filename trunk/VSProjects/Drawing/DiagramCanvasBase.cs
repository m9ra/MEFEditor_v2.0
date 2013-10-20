using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace Drawing
{
    public abstract class DiagramCanvasBase : Panel
    {
        private DiagramItem _ownerItem;

        private DiagramContext _context;

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

        internal void SetOwner(DiagramItem owner)
        {
            _ownerItem = owner;
            SetContext(owner.DiagramContext);
            Children.Clear();
        }

        internal void SetContext(DiagramContext context)
        {
            _context = context;
        }

        internal void AddJoin(JoinDrawing join)
        {
            Children.Add(join);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (_context != null)
            {
                _context.Provider.Engine.ArrangeChildren(_ownerItem, this);
            }

            foreach (FrameworkElement child in Children)
            {
                var position = GetPosition(child);
                child.Arrange(new Rect(position, child.DesiredSize));

                var item = child as DiagramItem;
                if (item == null)
                    continue;

                item.RefreshGlobal();
            }
            return arrangeSize;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            foreach (UIElement child in Children)
            {
                //no borders on child size
                child.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            }

            //canvas doesn't need no size itself
            return new Size(MinHeight, MinWidth);
        }

       

    }
}
