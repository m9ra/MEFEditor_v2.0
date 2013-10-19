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
    class FollowGlobalPosition
    {
      
        internal static void Attach(ConnectorDrawing connector, DisplayEngine engine, PositionUpdate update)
        {
            var item = connector.OwningItem;
            UpdateGlobalPosition.GlobalPositionChange.AddValueChanged(item, (e, args) =>
            {
                positionUpdate(connector, item,engine, update);
            });

            UpdateGlobalPosition.WidthChange.AddValueChanged(item, (e, args) =>
            {
                positionUpdate(connector, item,engine, update);
            });

            UpdateGlobalPosition.HeightChange.AddValueChanged(item, (e, args) =>
            {
                positionUpdate(connector, item, engine, update);
            });

            positionUpdate(connector, item, engine, update);
        }

        private static void positionUpdate(ConnectorDrawing connector, DiagramItem item,DisplayEngine engine, PositionUpdate update)
        {
            var connectPoint = new Point(-connector.ConnectPoint.X, -connector.ConnectPoint.Y);
            var relativePos = item.TranslatePoint(connectPoint, connector);
            var itemPos = engine.GetGlobalPosition(item);
            var finalPosition = new Point(itemPos.X - relativePos.X, itemPos.Y - relativePos.Y);
            update(finalPosition);
        }
    }
}
