using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

using System.ComponentModel;
using System.Windows.Controls;

namespace Drawing.Behaviours
{
    class FollowRelativePosition
    {
        private static readonly DependencyPropertyDescriptor PositionChange = DependencyPropertyDescriptor.FromProperty(DiagramCanvas.PositionProperty, typeof(UserControl));

        private static readonly DependencyPropertyDescriptor WidthChange = DependencyPropertyDescriptor.FromProperty(FrameworkElement.ActualWidthProperty, typeof(UserControl));

        private static readonly DependencyPropertyDescriptor HeightChange = DependencyPropertyDescriptor.FromProperty(FrameworkElement.ActualHeightProperty, typeof(UserControl));

        internal static void Attach(ConnectorDrawing connector, DisplayEngine engine, PositionUpdate update)
        {
            var item = engine.GetItem(connector.Definition.Reference);
            PositionChange.AddValueChanged(item, (e, args) =>
            {
                positionUpdate(connector, item,engine, update);
            });

            WidthChange.AddValueChanged(item, (e, args) =>
            {
                positionUpdate(connector, item,engine, update);
            });

            HeightChange.AddValueChanged(item, (e, args) =>
            {
                positionUpdate(connector, item, engine, update);
            });

            positionUpdate(connector, item, engine, update);
        }

        private static void positionUpdate(ConnectorDrawing connector, DiagramItem item,DisplayEngine engine, PositionUpdate update)
        {
            var connectPoint = new Point(-connector.ConnectPoint.X, -connector.ConnectPoint.Y);
            var relativePos = item.TranslatePoint(connectPoint, connector);
            var itemPos = engine.GetPosition(item);
            var finalPosition = new Point(itemPos.X - relativePos.X, itemPos.Y - relativePos.Y);
            update(finalPosition);
        }
    }
}
