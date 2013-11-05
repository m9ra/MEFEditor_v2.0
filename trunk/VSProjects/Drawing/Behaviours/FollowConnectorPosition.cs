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
    class FollowConnectorPosition
    {
        internal static void Attach(ConnectorDrawing connector, DisplayEngine engine, OnConnectorMove move)
        {
            var item = connector.OwningItem;
        /*    UpdateGlobalPosition.GlobalPositionChange.AddValueChanged(item, (e, args) =>
            {
                move(connector);
            });*/

            UpdateGlobalPosition.WidthChange.AddValueChanged(item, (e, args) =>
            {
                move(connector);
            });

            UpdateGlobalPosition.HeightChange.AddValueChanged(item, (e, args) =>
            {
                move(connector);
            });
        }
    }
}
