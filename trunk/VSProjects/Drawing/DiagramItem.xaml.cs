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

        private readonly List<DiagramItem> _children = new List<DiagramItem>();

        internal IEnumerable<DiagramItem> Children { get { return _children; } }

        internal bool IsRootItem { get { return ParentItem == null; } }

        internal readonly DiagramItem ParentItem;

        public readonly DiagramContext DiagramContext;

        public readonly DiagramItemDefinition Definition;

        public IEnumerable<ConnectorDefinition> ConnectorDefinitions
        {
            get
            {
                return DiagramContext.Diagram.GetConnectorDefinitions(Definition);
            }
        }

        internal DiagramItem(DiagramItemDefinition definition, DiagramContext diagramContext)
        {
            Definition = definition;
            DiagramContext = diagramContext;

            InitializeComponent();
        }

        internal DiagramItem(DiagramItemDefinition definition, DiagramItem parentItem)
        {
            Definition = definition;
            ParentItem = parentItem;
            ParentItem._children.Add(this);
            DiagramContext = parentItem.DiagramContext;

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

        public void FillSlot(SlotCanvas canvas, SlotDefinition slot)
        {
            canvas.SetOwner(this);
            foreach (var itemReference in slot.References)
            {
                var itemDefinition = DiagramContext.Diagram.GetItemDefinition(itemReference.DefinitionID);
                var item = new DiagramItem(itemDefinition, this);
                var itemContext = DiagramContext.Provider.DrawItem(item);
                canvas.Children.Add(item);
            }
        }

        internal void RefreshGlobal()
        {
            var position = computeGlobalPosition();
            DiagramCanvas.SetGlobalPosition(this, position);
        }

        private Point computeGlobalPosition()
        {
            var output = DiagramContext.Provider.Output;
            return this.TranslatePoint(new Point(0, 0), output);
        }
    }
}
