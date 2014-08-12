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

using MEFEditor.Drawing.ArrangeEngine;

namespace MEFEditor.Drawing
{
    /// <summary>
    /// Interaction logic for TestControl.xaml.
    /// </summary>
    public partial class DiagramItem : UserControl
    {
        /// <summary>
        /// The is highlighted flag.
        /// </summary>
        private bool _isHighlighted;

        /// <summary>
        /// The items connectors.
        /// </summary>
        private readonly Dictionary<ConnectorDefinition, ConnectorDrawing> _connectors = new Dictionary<ConnectorDefinition, ConnectorDrawing>();

        /// <summary>
        /// The children.
        /// </summary>
        private readonly List<DiagramItem> _children = new List<DiagramItem>();

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <value>The children.</value>
        internal IEnumerable<DiagramItem> Children { get { return _children; } }

        /// <summary>
        /// Gets a value indicating whether this instance is root item.
        /// </summary>
        /// <value><c>true</c> if this instance is root item; otherwise, <c>false</c>.</value>
        internal bool IsRootItem { get { return ParentItem == null; } }

        /// <summary>
        /// Gets the parent identifier.
        /// </summary>
        /// <value>The parent identifier.</value>
        internal string ParentID { get { return IsRootItem ? "" : ParentItem.ID; } }

        /// <summary>
        /// Gets or sets the position cursor.
        /// </summary>
        /// <value>The position cursor.</value>
        internal PositionCursor PositionCursor { get; set; }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        internal string ID { get { return Definition.ID; } }

        /// <summary>
        /// The parent item.
        /// </summary>
        internal readonly DiagramItem ParentItem;

        /// <summary>
        /// Gets the output drawing.
        /// </summary>
        /// <value>The output.</value>
        internal DiagramCanvas Output { get { return DiagramContext.Provider.Engine.Output; } }

        /// <summary>
        /// The containing diagram canvas.
        /// </summary>
        internal readonly DiagramCanvasBase ContainingDiagramCanvas;

        /// <summary>
        /// Gets the parent exclude edit.
        /// </summary>
        /// <value>The parent exclude edit.</value>
        internal EditDefinition ParentExcludeEdit { get; private set; }

        /// <summary>
        /// The accept edits.
        /// </summary>
        internal readonly List<EditDefinition> AcceptEdits = new List<EditDefinition>();

        /// <summary>
        /// Gets a value indicating whether this instance can exclude from parent.
        /// </summary>
        /// <value><c>true</c> if this instance can be excluded from parent; otherwise, <c>false</c>.</value>
        internal bool CanExcludeFromParent { get { return ParentExcludeEdit != null; } }

        #region Public API for drawing extension implementors

        /// <summary>
        /// The diagram context where item is displayed.
        /// </summary>
        public readonly DiagramContext DiagramContext;

        /// <summary>
        /// The definition current diagram item.
        /// </summary>
        public readonly DiagramItemDefinition Definition;

        /// <summary>
        /// Determine that item is recursive.
        /// </summary>
        public bool IsRecursive;

        /// <summary>
        /// Gets a value indicating whether this instance has position.
        /// </summary>
        /// <value><c>true</c> if this instance has position; otherwise, <c>false</c>.</value>
        public bool HasPosition { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is highlighted.
        /// </summary>
        /// <value><c>true</c> if this instance is highlighted; otherwise, <c>false</c>.</value>
        public bool IsHighlighted
        {
            get
            {
                return _isHighlighted;
            }

            internal set
            {
                if (_isHighlighted == value)
                    return;

                _isHighlighted = value;
                if (_isHighlighted)
                    DiagramContext.HighlightedItem = this;
            }
        }

        /// <summary>
        /// Gets the top connector drawings.
        /// </summary>
        /// <value>The top connector drawings.</value>
        public IEnumerable<ConnectorDrawing> TopConnectorDrawings
        {
            get
            {
                return getConnectorDrawings(ConnectorAlign.Top);
            }
        }

        /// <summary>
        /// Gets the bottom connector drawings.
        /// </summary>
        /// <value>The bottom connector drawings.</value>
        public IEnumerable<ConnectorDrawing> BottomConnectorDrawings
        {
            get
            {
                return getConnectorDrawings(ConnectorAlign.Bottom);
            }
        }

        /// <summary>
        /// Gets the left connector drawings.
        /// </summary>
        /// <value>The left connector drawings.</value>
        public IEnumerable<ConnectorDrawing> LeftConnectorDrawings
        {
            get
            {
                return getConnectorDrawings(ConnectorAlign.Left);
            }
        }

        /// <summary>
        /// Gets the right connector drawings.
        /// </summary>
        /// <value>The right connector drawings.</value>
        public IEnumerable<ConnectorDrawing> RightConnectorDrawings
        {
            get
            {
                return getConnectorDrawings(ConnectorAlign.Right);
            }
        }

        /// <summary>
        /// Gets the connector definitions of contained connectors.
        /// </summary>
        /// <value>The connector definitions.</value>
        public IEnumerable<ConnectorDefinition> ConnectorDefinitions
        {
            get
            {
                return DiagramContext.Diagram.GetConnectorDefinitions(Definition);
            }
        }

        /// <summary>
        /// Gets the global position of current item.
        /// </summary>
        /// <value>The global position.</value>
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
                HasPosition = true;
                var localPos = AsLocalPosition(value);
                DiagramCanvas.SetPosition(this, localPos);
            }
        }

        /// <summary>
        /// Gets the local position of current item.
        /// </summary>
        /// <value>The local position.</value>
        public Point LocalPosition
        {
            get
            {
                return DiagramCanvas.GetPosition(this);
            }
        }

        /// <summary>
        /// Get global position computed to local coordinates.
        /// </summary>
        /// <param name="globalPosition">The global position.</param>
        /// <returns>Local position.</returns>
        internal Point AsLocalPosition(Point globalPosition)
        {
            var diff = GlobalPosition - globalPosition;

            var localPos = DiagramCanvas.GetPosition(this);
            localPos.X -= diff.X;
            localPos.Y -= diff.Y;

            return localPos;
        }

        /// <summary>
        /// Fills the slot canvas with instances according to given slot definition.
        /// </summary>
        /// <param name="slotCanvas">The slot canvas that will be filled.</param>
        /// <param name="slot">The slot definition.</param>
        public void FillSlot(SlotCanvas slotCanvas, SlotDefinition slot)
        {
            //recursive check is required only for diagram items 
            //filling some slots - only some of its children can be recursive
            slotCanvas.SetOwner(this);

            var ancestors = new HashSet<DiagramItemDefinition>();
            var current = this;
            while (current != null)
            {
                ancestors.Add(current.Definition);
                current = current.ParentItem;
            }

            foreach (var itemReference in slot.References)
            {
                var itemDefinition = DiagramContext.Diagram.GetItemDefinition(itemReference.DefinitionID);

                var item = new DiagramItem(itemDefinition, this, slotCanvas);
                item.IsRecursive = ancestors.Contains(itemDefinition);
                DiagramContext.Provider.InitializeItemDrawing(item);
                slotCanvas.Children.Add(item);
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramItem" /> class.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="diagramContext">The diagram context.</param>
        internal DiagramItem(DiagramItemDefinition definition, DiagramContext diagramContext)
        {
            Definition = definition;
            DiagramContext = diagramContext;
            ContainingDiagramCanvas = Output;

            initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramItem" /> class.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="parentItem">The parent item.</param>
        /// <param name="slot">The slot.</param>
        internal DiagramItem(DiagramItemDefinition definition, DiagramItem parentItem, SlotCanvas slot)
        {
            ContainingDiagramCanvas = slot;
            Definition = definition;
            ParentItem = parentItem;
            ParentItem._children.Add(this);
            DiagramContext = parentItem.DiagramContext;

            initialize();
        }

        /// <summary>
        /// Attach the specified connector.
        /// </summary>
        /// <param name="connector">The connector.</param>
        /// <exception cref="System.NotSupportedException">Given align is not supported</exception>
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

        /// <summary>
        /// Sets the content.
        /// </summary>
        /// <param name="content">The content.</param>
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

        /// <summary>
        /// Gets the connector.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>ConnectorDrawing.</returns>
        internal ConnectorDrawing GetConnector(ConnectorDefinition point)
        {
            return _connectors[point];
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void initialize()
        {
            if (Definition.GlobalPosition.HasValue)
                GlobalPosition = Definition.GlobalPosition.Value;

            if (IsRootItem)
            {
                DiagramCanvasBase.SetPosition(this, GlobalPosition);
            }

            InitializeComponent();

            setEdits();
        }

        /// <summary>
        /// Sets the edits.
        /// </summary>
        private void setEdits()
        {
            var menu = new ContextMenu();

            //add instance edits
            foreach (var edit in Definition.Edits)
            {
                addMenuEdit(menu, edit);
            }

            foreach (var command in Definition.Commands)
            {
                addMenuCommand(menu, command);
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

        /// <summary>
        /// Adds the menu edit.
        /// </summary>
        /// <param name="menu">The menu.</param>
        /// <param name="edit">The edit.</param>
        /// <exception cref="System.NotSupportedException">Cannot specify multiple exclude edits</exception>
        private void addMenuEdit(ContextMenu menu, EditDefinition edit)
        {
            if (!edit.IsActive(DiagramContext.Diagram.InitialView))
                return;


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

                    item.Click += (e, s) =>
                    {
                        try
                        {
                            edit.Commit(DiagramContext.Diagram.InitialView);
                        }
                        catch (Exception) { }
                    };
                    break;
            }
        }

        /// <summary>
        /// Gets the connector drawings.
        /// </summary>
        /// <param name="align">The align.</param>
        /// <returns>IEnumerable&lt;ConnectorDrawing&gt;.</returns>
        private IEnumerable<ConnectorDrawing> getConnectorDrawings(ConnectorAlign align)
        {
            return _connectors.Values.Where((connector) => connector.Align == align);
        }

        /// <summary>
        /// Adds the menu command.
        /// </summary>
        /// <param name="menu">The menu.</param>
        /// <param name="command">The command.</param>
        private void addMenuCommand(ContextMenu menu, CommandDefinition command)
        {
            var item = new MenuItem();
            item.Header = command.Name;
            menu.Items.Add(item);

            item.Click += (e, s) => command.Command();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("Item: {0}", Definition.ID);
        }

        /// <summary>
        /// Determine that point is out of item bounds.
        /// </summary>
        /// <param name="globalPosition">The global position.</param>
        /// <returns><c>true</c> if global position is out of bounds, <c>false</c> otherwise.</returns>
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
