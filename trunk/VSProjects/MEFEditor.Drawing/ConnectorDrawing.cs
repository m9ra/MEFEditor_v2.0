using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace MEFEditor.Drawing
{
    public enum ConnectorAlign { Top, Bottom, Left, Right }

    public abstract class ConnectorDrawing : Border
    {
        public readonly ConnectorAlign Align;
        public readonly DiagramItem OwningItem;
        public readonly ConnectorDefinition Definition;
        public abstract Point ConnectPoint { get; }

        public ConnectorDrawing(ConnectorDefinition definition, ConnectorAlign align, DiagramItem owningItem)
        {
            Align = align;
            Definition = definition;
            OwningItem = owningItem;

            var halfMargin = 10;
            switch (Align)
            {
                case ConnectorAlign.Top:
                case ConnectorAlign.Bottom:
                    Margin = new Thickness(halfMargin, 0, halfMargin, 0);
                    break;
                case ConnectorAlign.Left:
                case ConnectorAlign.Right:
                    Margin = new Thickness(0, halfMargin, 0, halfMargin);
                    break;
            }
        }

        internal Point OutOfItemPoint
        {
            //TODO connect with item margin settings
            get
            {
                var point = GlobalConnectPoint;
                var shift = 40;
                switch (Align)
                {
                    case ConnectorAlign.Top:
                        point.Y -= shift;
                        break;
                    case ConnectorAlign.Bottom:
                        point.Y += shift;
                        break;
                    case ConnectorAlign.Left:
                        point.X -= shift;
                        break;
                    case ConnectorAlign.Right:
                        point.X += shift;
                        break;
                }

                return GlobalConnectPoint;
            }
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
