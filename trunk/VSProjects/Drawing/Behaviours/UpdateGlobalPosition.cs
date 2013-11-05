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
  //      internal static readonly DependencyPropertyDescriptor GlobalPositionChange = DependencyPropertyDescriptor.FromProperty(DiagramCanvas.GlobalPositionProperty, typeof(UserControl));

        internal static readonly DependencyPropertyDescriptor WidthChange = DependencyPropertyDescriptor.FromProperty(FrameworkElement.ActualWidthProperty, typeof(UserControl));

        internal static readonly DependencyPropertyDescriptor HeightChange = DependencyPropertyDescriptor.FromProperty(FrameworkElement.ActualHeightProperty, typeof(UserControl));

        internal static readonly DependencyPropertyDescriptor PositionChange = DependencyPropertyDescriptor.FromProperty(DiagramCanvasBase.PositionProperty, typeof(UserControl));

        public static void Attach(DiagramItem item)
        {
        /*    GlobalPositionChange.AddValueChanged(item, (s, e) =>
            {
                item.HasGlobalPositionChange = true;
                //global position of parent has changed - may affect children's global position
                foreach (var child in item.Children)
                {
                    child.RefreshGlobal();
                }
            });*/
        }

    }
}
