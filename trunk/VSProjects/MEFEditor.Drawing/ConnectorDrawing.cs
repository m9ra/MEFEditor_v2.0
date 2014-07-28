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
    /// <summary>
    /// Determine where connector is aligned at <see cref="DiagramItem" />.
    /// </summary>
    public enum ConnectorAlign
    {
        /// <summary>
        /// The top align.
        /// </summary>
        Top,
        /// <summary>
        /// The bottom align.
        /// </summary>
        Bottom,
        /// <summary>
        /// The left align.
        /// </summary>
        Left,
        /// <summary>
        /// The right align.
        /// </summary>
        Right
    }

    /// <summary>
    /// Base of connector drawing.
    /// </summary>
    public abstract class ConnectorDrawing : Border
    {
        /// <summary>
        /// The align of connector definition.
        /// </summary>
        public readonly ConnectorAlign Align;
        /// <summary>
        /// The owning item.
        /// </summary>
        public readonly DiagramItem OwningItem;
        /// <summary>
        /// The definition of connector.
        /// </summary>
        public readonly ConnectorDefinition Definition;
        /// <summary>
        /// Gets the connect point.
        /// </summary>
        /// <value>The connect point.</value>
        public abstract Point ConnectPoint { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorDrawing" /> class.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="align">The align.</param>
        /// <param name="owningItem">The owning item.</param>
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
               
        /// <summary>
        /// Gets the global connect point.
        /// </summary>
        /// <value>The global connect point.</value>
        public Point GlobalConnectPoint
        {
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
