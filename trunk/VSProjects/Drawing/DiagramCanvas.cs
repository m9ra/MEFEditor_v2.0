using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Windows;
using System.ComponentModel;


namespace Drawing
{
    public class DiagramCanvas:DiagramCanvasBase
    {
        #region GlobalPosition property

        public static readonly DependencyProperty GlobalPositionProperty =
            DependencyProperty.RegisterAttached("GlobalPosition", typeof(Point),
            typeof(DiagramCanvas), new FrameworkPropertyMetadata(new Point(0, 0),
            FrameworkPropertyMetadataOptions.AffectsParentArrange));

        public static void SetGlobalPosition(UIElement element, Point position)
        {
            var pos = GetGlobalPosition(element);
            if (pos == position)
                return;

            element.SetValue(GlobalPositionProperty, position);
        }

        public static Point GetGlobalPosition(UIElement element)
        {
            return (Point)element.GetValue(GlobalPositionProperty);
        }

        #endregion

    }
}
