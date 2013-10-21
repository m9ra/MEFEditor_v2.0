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
    public abstract class ConnectorDrawing:Border
    {
        public readonly DiagramItem OwningItem;
        public readonly ConnectorDefinition Definition;
        public abstract Point ConnectPoint { get; }

        public ConnectorDrawing(ConnectorDefinition definition,DiagramItem owningItem)
        {
            Definition = definition;
            OwningItem = owningItem;
        }

        public Point GlobalConnectPoint
        {
            //TODO try cache this value
            get
            {
                var connectPoint = new Point(-ConnectPoint.X, -ConnectPoint.Y);
                var relativePos = OwningItem.TranslatePoint(connectPoint, this);
                var itemPos = OwningItem.GlobalPosition;
                var finalPosition = new Point(itemPos.X - relativePos.X, itemPos.Y - relativePos.Y);
                return finalPosition;
            }
        }
    }
}
