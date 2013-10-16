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
        public readonly ConnectorDefinition Definition;
        public abstract Point ConnectPoint { get; }

        public ConnectorDrawing(ConnectorDefinition definition)
        {
            Definition = definition;
        }
    }
}
