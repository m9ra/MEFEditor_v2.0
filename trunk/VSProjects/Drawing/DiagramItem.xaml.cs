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

        internal string ParentID { get { return IsRootItem ? "" : ParentItem.ID; } }

        internal string ID { get { return Definition.ID; } }

        internal readonly DiagramItem ParentItem;

        internal DiagramCanvas Output { get { return DiagramContext.Provider.Engine.Output; } }

        internal readonly DiagramCanvasBase ContainingDiagramCanvas;

        internal EditDefinition ParentExcludeEdit { get; private set; }

        internal readonly List<EditDefinition> AcceptEdits = new List<EditDefinition>();

        internal bool CanExcludeFromParent { get { return ParentExcludeEdit != null; } }

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
                var localPos = DiagramCanvas.GetPosition(this);

                var parentGlobal = ContainingDiagramCanvas.GlobalPosition;
                localPos.X += parentGlobal.X;
                localPos.Y += parentGlobal.Y;

                return localPos;
            }

            internal set
            {
                var localPos = AsLocalPosition(value);
                DiagramCanvas.SetPosition(this, localPos);
            }
        }

        internal Point AsLocalPosition(Point globalPosition)
        {
            var diff = GlobalPosition - globalPosition;

            var localPos = DiagramCanvas.GetPosition(this);
            localPos.X -= diff.X;
            localPos.Y -= diff.Y;
       
            return localPos;
        }

        public void FillSlot(SlotCanvas slotCanvas, SlotDefinition slot)
        {
            slotCanvas.SetOwner(this);
            foreach (var itemReference in slot.References)
            {
                var itemDefinition = DiagramContext.Diagram.GetItemDefinition(itemReference.DefinitionID);
                var item = new DiagramItem(itemDefinition, this, slotCanvas);
                var itemContext = DiagramContext.Provider.DrawItem(item);
                slotCanvas.Children.Add(item);
            }
        }

        #endregion

        internal DiagramItem(DiagramItemDefinition definition, DiagramContext diagramContext)
        {
            Definition = definition;
            DiagramContext = diagramContext;
            ContainingDiagramCanvas = Output;

            initialize();
        }

        internal DiagramItem(DiagramItemDefinition definition, DiagramItem parentItem, SlotCanvas slot)
        {
            ContainingDiagramCanvas = slot;
            Definition = definition;
            ParentItem = parentItem;
            ParentItem._children.Add(this);
            DiagramContext = parentItem.DiagramContext;

            initialize();
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

        internal void SetContent(ContentDrawing content)
        {
            ContentDrawing.Content = content;

            //set margins for connectors - for radius corners is needed to shift its position
            var cr = content.CornerRadius;

            LeftConnectors.Margin = new Thickness(0, cr.TopLeft, 0, cr.BottomLeft);
            RightConnectors.Margin = new Thickness(0, cr.TopRight, 0, cr.BottomRight);

            TopConnectors.Margin = new Thickness(cr.TopLeft, 0, cr.TopRight, 0);
            BottomConnectors.Margin = new Thickness(cr.BottomLeft, 0, cr.BottomRight, 0);
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

        private void addMenuEdit(ContextMenu menu, EditDefinition edit)
        {
            switch (edit.Name)
            {
                case ".exclude":
                    if (ParentExcludeEdit != null)
                        throw new NotSupportedException("Cannot specify multiple exclude edits");
                    ParentExcludeEdit = edit;
                    break;
                case ".accept":
                    AcceptEdits.Add(edit);
                    break;
                default:
                    var item = new MenuItem();
                    item.Header = edit.Name;
                    menu.Items.Add(item);

                    item.Click += (e, s) => edit.Commit(DiagramContext.Diagram.InitialView);
                    break;
            }
        }

        public override string ToString()
        {
            return string.Format("Item: {0}", Definition.ID);
        }

        internal bool OutOfBounds(ref Point globalPosition)
        {
            if (IsRootItem)
                return false;

            //compute boundaries on containing slot
            var minPos = ContainingDiagramCanvas.GlobalPosition;

            var boundsSize = ContainingDiagramCanvas.DesiredSize;
            var boundMargins = ContainingDiagramCanvas.Margin;

            var maxX = minPos.X + boundsSize.Width - DesiredSize.Width - boundMargins.Left - boundMargins.Right;
            var maxY = minPos.Y + boundsSize.Height - DesiredSize.Height - boundMargins.Top - boundMargins.Bottom;

            //compute bounded position
            var outOfBounds = false;


            if (globalPosition.X > maxX)
            {
                globalPosition.X = maxX;
                outOfBounds = true;
            }

            if (globalPosition.Y > maxY)
            {
                globalPosition.Y = maxY;
                outOfBounds = true;
            }

            if (globalPosition.X < minPos.X)
            {
                globalPosition.X = minPos.X;
                outOfBounds = true;
            }

            if (globalPosition.Y < minPos.Y)
            {
                globalPosition.Y = minPos.Y;
                outOfBounds = true;
            }

            return outOfBounds;
        }
    }
}
