using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;

namespace Drawing.Behaviours
{
    class UpdateGlobalPosition
    {
        internal static readonly DependencyPropertyDescriptor GlobalPositionChange = DependencyPropertyDescriptor.FromProperty(DiagramCanvas.GlobalPositionProperty, typeof(UserControl));

        internal static readonly DependencyPropertyDescriptor WidthChange = DependencyPropertyDescriptor.FromProperty(FrameworkElement.ActualWidthProperty, typeof(UserControl));

        internal static readonly DependencyPropertyDescriptor HeightChange = DependencyPropertyDescriptor.FromProperty(FrameworkElement.ActualHeightProperty, typeof(UserControl));

        internal static readonly DependencyPropertyDescriptor PositionChange = DependencyPropertyDescriptor.FromProperty(DiagramCanvasBase.PositionProperty, typeof(UserControl));

        public static void Attach(DiagramItem item)
        {
            PositionChange.AddValueChanged(item, (s, e) =>
            {
                //relative position has changed - may affect global position
                refreshPosition(item);
            });

            WidthChange.AddValueChanged(item, (s, e) =>
            {
                //relative position has changed - may affect global position
                refreshPosition(item);
            });

            HeightChange.AddValueChanged(item, (s, e) =>
            {
                //relative position has changed - may affect global position
                refreshPosition(item);
            });

            GlobalPositionChange.AddValueChanged(item, (s, e) =>
            {
                //global position of parent has changed - may affect children's global position
                foreach (var child in item.Children)
                {
                    refreshPosition(child);
                }
            });
        }

        private static Point refreshPosition(DiagramItem item)
        {
            var position = getGlobalPosition(item);
            DiagramCanvas.SetGlobalPosition(item, position);

            return position;
        }

        private static Point getGlobalPosition(DiagramItem item)
        {
            var output = item.DiagramContext.Provider.Output;
            return item.TranslatePoint(new Point(0, 0), output);
        }

    }
}
