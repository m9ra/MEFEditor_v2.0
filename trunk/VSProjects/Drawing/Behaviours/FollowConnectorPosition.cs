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
    /// <summary>
    /// Handler for connector move events
    /// </summary>
    /// <param name="connector">Connector that has been moved</param>
    delegate void OnConnectorMove(ConnectorDrawing connector);

    /// <summary>
    /// Definition of connector's following behaviour
    /// </summary>
    internal class FollowConnectorPosition
    {
        /// <summary>
        /// Attach behaviour to given connector
        /// </summary>
        /// <param name="connector">Attached connector</param>
        /// <param name="engine">Engine using attached behaviour</param>
        /// <param name="moveHandler">Handler used for move events</param>
        internal static void Attach(ConnectorDrawing connector, DisplayEngine engine, OnConnectorMove moveHandler)
        {
            var item = connector.OwningItem;

            UpdateGlobalPosition.WidthChange.AddValueChanged(item, (e, args) =>
            {
                moveHandler(connector);
            });

            UpdateGlobalPosition.HeightChange.AddValueChanged(item, (e, args) =>
            {
                moveHandler(connector);
            });
        }
    }
}
