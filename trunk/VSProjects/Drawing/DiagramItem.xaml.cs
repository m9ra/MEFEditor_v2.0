using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Drawing
{
    /// <summary>
    /// Interaction logic for TestControl.xaml
    /// </summary>
    public partial class DiagramItem : UserControl
    {
        private readonly Dictionary<ConnectorDefinition, ConnectorDrawing> _connectors = new Dictionary<ConnectorDefinition, ConnectorDrawing>();

        public readonly DrawingDefinition Definition;

        internal DiagramItem(DrawingDefinition definition)
        {
            Definition = definition;

            InitializeComponent();      
        }

        internal void Attach(ConnectorDrawing connector)
        {
            _connectors.Add(connector.Definition, connector);
            Connectors.Children.Add(connector);
        }

        internal void SetContent(FrameworkElement content)
        {
            ContentDrawing.Content = content;
        }

        internal ConnectorDrawing GetConnector(ConnectorDefinition point)
        {
            return _connectors[point];
        }
    }
}
