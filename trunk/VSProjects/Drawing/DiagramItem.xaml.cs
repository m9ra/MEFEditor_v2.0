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
        private readonly Dictionary<JoinPointDefinition, Connector> _connectors = new Dictionary<JoinPointDefinition, Connector>();

        public readonly DrawingDefinition Definition;

        internal Point LastDragPosition;
        internal bool IsDragStarted;

        public DiagramItem(DrawingDefinition definition)
        {
            Definition = definition;

            InitializeComponent();
            TypeName.Text = definition.DrawedType;

            foreach (var property in definition.Properties)
            {
                var propertyBlock = new TextBlock();
                propertyBlock.Text = string.Format("{0}: {1}", property.Name, property.Value);

                Properties.Children.Add(propertyBlock);
            }
        }

        internal void Attach(Connector connector)
        {
            _connectors.Add(connector.Definition, connector);
            Connectors.Children.Add(connector);
        }

        internal Connector GetConnector(JoinPointDefinition point)
        {
            return _connectors[point];
        }
    }
}
