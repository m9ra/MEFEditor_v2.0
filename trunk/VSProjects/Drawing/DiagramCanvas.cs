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
    public class DiagramCanvas : Panel
    {
        #region Position property

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.RegisterAttached("Position", typeof(Point),
            typeof(DiagramCanvas), new FrameworkPropertyMetadata(new Point(0, 0),
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

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            foreach (UIElement child in Children)
            {                   
                var position = GetPosition(child);
                child.Arrange(new Rect(position, child.DesiredSize));
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
            return new Size(0, 0);
        }

        internal void AddJoin(JoinDrawing join)
        {
            Children.Add(join);
        }
    }
}
