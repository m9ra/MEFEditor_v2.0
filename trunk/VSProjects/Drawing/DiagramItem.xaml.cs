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

        internal bool HasGlobalPositionChange;

        internal IEnumerable<DiagramItem> Children { get { return _children; } }

        internal bool IsRootItem { get { return ParentItem == null; } }

        internal string ParentID { get { return IsRootItem ? "" : ParentItem.ID; } }

        internal string ID { get { return Definition.ID; } }

        internal readonly DiagramItem ParentItem;

        #region Public API for drawing extension implementors

        public readonly DiagramContext DiagramContext;

        public readonly DiagramItemDefinition Definition;

        public IEnumerable<ConnectorDefinition> ConnectorDefinitions
        {
            get
            {
                return DiagramContext.Diagram.GetConnectorDefinitions(Definition);
            }
        }

        public Point GlobalPosition
        {
            get
            {
                return DiagramCanvas.GetGlobalPosition(this);
            }

            private set
            {
                DiagramCanvas.SetGlobalPosition(this, value);
            }
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

        #endregion

        internal DiagramItem(DiagramItemDefinition definition, DiagramContext diagramContext)
        {
            Definition = definition;
            DiagramContext = diagramContext;

            initialize();
        }

        internal DiagramItem(DiagramItemDefinition definition, DiagramItem parentItem)
        {
            Definition = definition;
            ParentItem = parentItem;
            ParentItem._children.Add(this);
            DiagramContext = parentItem.DiagramContext;

            initialize();
        }

        internal void RefreshGlobal()
        {
            var position = computeGlobalPosition();
            DiagramCanvas.SetGlobalPosition(this, position);
        }

        internal void Attach(ConnectorDrawing connector)
        {
            _connectors.Add(connector.Definition, connector);


            StackPanel connectors;
            switch (connector.Align)
            {
                case ConnectorAlign.Top:
                    connectors = TopConnectors;
                    break;
                case ConnectorAlign.Bottom:
                    connectors = BottomConnectors;
                    break;
                case ConnectorAlign.Left:
                    connectors = LeftConnectors;
                    break;
                case ConnectorAlign.Right:
                    connectors = RightConnectors;
                    break;
                default:
                    throw new NotSupportedException("Given align is not supported");
            }
            connectors.Children.Add(connector);
        }

        internal void SetContent(FrameworkElement content)
        {
            ContentDrawing.Content = content;
        }

        internal ConnectorDrawing GetConnector(ConnectorDefinition point)
        {
            return _connectors[point];
        }

        private void initialize()
        {
            GlobalPosition = Definition.GlobalPosition;

            if (IsRootItem)
            {
                DiagramCanvasBase.SetPosition(this, GlobalPosition);
            }

            InitializeComponent();

            setEdits();
        }

        private void setEdits()
        {
            var menu = new ContextMenu();

            //add instance edits
            foreach (var edit in Definition.Edits)
            {
                addMenuEdit(menu, edit);
            }

            if (ParentItem != null)
            {
                //add attached edits
                foreach (var edit in Definition.GetAttachedEdits(ParentItem.Definition.ID))
                {
                    addMenuEdit(menu, edit);
                }
            }


            ContentDrawing.ContextMenu = menu;
            if (menu.Items.Count == 0)
                menu.Visibility = System.Windows.Visibility.Hidden;
        }

        private static void addMenuEdit(ContextMenu menu, EditDefinition edit)
        {
            var item = new MenuItem();
            item.Header = edit.Name;
            menu.Items.Add(item);

            item.Click += (e, s) => edit.Action();
        }

        private Point computeGlobalPosition()
        {
            var output = DiagramContext.Provider.Output;
            return this.TranslatePoint(new Point(0, 0), output);
        }

        public override string ToString()
        {
            return string.Format("Item: {0}", Definition.ID);
        }
    }
}
