using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Utilities;

using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.ComponentModel;


namespace Drawing
{
    delegate void PositionUpdate(Point position);

    class DisplayEngine
    {

        private readonly DependencyPropertyDescriptor PositionChange = DependencyPropertyDescriptor.FromProperty(DiagramCanvas.PositionProperty,typeof(UserControl));
        private readonly Dictionary<string, HashSet<JoinDefinition>> _affectedJoins = new Dictionary<string, HashSet<JoinDefinition>>();

        private readonly Dictionary<string, DiagramItem> _items = new Dictionary<string, DiagramItem>();

        private readonly Dictionary<JoinDefinition, Line> _joins = new Dictionary<JoinDefinition, Line>();

        private readonly LinkedList<DiagramItem> _itemsZOrdering = new LinkedList<DiagramItem>();

        private readonly DiagramCanvas _output;

        internal DisplayEngine(DiagramCanvas output)
        {
            _output = output;
        }

        internal void AddItem(DiagramItem item)
        {
            hookHandlers(item);
            _items.Add(item.Definition.ID, item);
            _itemsZOrdering.AddFirst(item);
        }

        internal void AddJoin(JoinDefinition joinDefinition)
        {
            var from = getConnector(joinDefinition.From);
            var to = getConnector(joinDefinition.To);

            var join = new Line();
            join.StrokeThickness = 2;
            join.Stroke = Brushes.Orange;
            _joins[joinDefinition] = join;

            registerAffectedConnector(from, joinDefinition);
            registerAffectedConnector(to, joinDefinition);


            registerPositionChange(from, (p) =>
            {
                join.X1 = p.X;
                join.Y1 = p.Y;
            });

            registerPositionChange(to, (p) =>
            {
                join.X2 = p.X;
                join.Y2 = p.Y;
            });

            _output.AddJoin(join);
        }

        internal void Display()
        {
            applyZOrdering();
            foreach (var item in _items.Values)
            {
                _output.Children.Add(item);
            }
        }

        internal void Clear()
        {
            _items.Clear();
            _output.Children.Clear();
        }

        internal void SetPosition(DiagramItem item, Point position)
        {
            DiagramCanvas.SetPosition(item, position);
        }

        internal Point GetPosition(DiagramItem item)
        {
            return DiagramCanvas.GetPosition(item);
        }

        private void registerAffectedConnector(Connector connector, JoinDefinition definition)
        {
            var id = connector.Definition.Reference.DefinitionID;

            HashSet<JoinDefinition> joins;
            if (!_affectedJoins.TryGetValue(id, out joins))
            {
                joins = new HashSet<JoinDefinition>();
                _affectedJoins[id] = joins;
            }

            joins.Add(definition);
        }

        private void registerPositionChange(Connector connector, PositionUpdate update)
        {
            var id = connector.Definition.Reference.DefinitionID;

            var item = _items[id];
            
            PositionChange.AddValueChanged(item,(e,args)=>{
            /*    var position = _output.TranslatePoint(connector.ConnectPoint, connector);
                update(new Point(-position.X,-position.Y));*/
                update(GetPosition(item));
            });
        }

        private Connector getConnector(JoinPointDefinition joinPointDefinition)
        {
            var item = _items[joinPointDefinition.Reference.DefinitionID];
            return item.GetConnector(joinPointDefinition);
        }

        private void hookHandlers(DiagramItem item)
        {
            item.MouseMove += (source, e) => onMouseMove(item, e);
            item.MouseDown += (source, e) => onMouseDown(item, e);
            item.MouseUp += (source, e) => onMouseUp(item, e);
        }

        private void sendFront(DiagramItem item)
        {
            _itemsZOrdering.Remove(item);
            _itemsZOrdering.AddFirst(item);

            applyZOrdering();
        }

        private void applyZOrdering()
        {
            var currentIndex = 0;
            foreach (var item in _itemsZOrdering)
            {
                --currentIndex;

                DiagramCanvas.SetZIndex(item, currentIndex);
            }
        }

        #region Drag drop moving operation

        void onMouseMove(DiagramItem item, MouseEventArgs e)
        {
            if (!item.IsDragStarted)
                return;

            var currentMousePos = e.GetPosition(null);
            var shift = currentMousePos - item.LastDragPosition;
            item.LastDragPosition = currentMousePos;

            var currentItemPos = GetPosition(item);

            currentItemPos += shift;
            SetPosition(item, currentItemPos);
        }

        private void onMouseDown(DiagramItem item, MouseButtonEventArgs e)
        {
            item.CaptureMouse();
            sendFront(item);
            item.LastDragPosition = e.GetPosition(null);
            item.IsDragStarted = true;
        }

        private void onMouseUp(DiagramItem item, MouseButtonEventArgs e)
        {
            item.IsDragStarted = false;
            item.ReleaseMouseCapture();
        }

        #endregion
    }
}
