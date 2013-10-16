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


using Drawing.Behaviours;

namespace Drawing
{
    delegate void PositionUpdate(Point position);

    class DisplayEngine
    {
        private readonly Dictionary<string, HashSet<JoinDefinition>> _affectedJoins = new Dictionary<string, HashSet<JoinDefinition>>();

        private readonly Dictionary<string, DiagramItem> _items = new Dictionary<string, DiagramItem>();

        private readonly Dictionary<JoinDefinition, Line> _joins = new Dictionary<JoinDefinition, Line>();

        private readonly DiagramCanvas _output;

        ElementGroup _orderingGroup = new ElementGroup();

        internal DisplayEngine(DiagramCanvas output)
        {
            _output = output;
        }

        #region Public API

        public void Display()
        {
            foreach (var item in _items.Values)
            {
                _output.Children.Add(item);
            }
        }

        public void Clear()
        {
            _orderingGroup = new ElementGroup();
            _items.Clear();
            _output.Children.Clear();
        }

        #endregion

        #region Display building methods

        internal void AddItem(DiagramItem item)
        {
            DragAndDrop.Attach(item, GetPosition, SetPosition);
            ZOrdering.Attach(item, _orderingGroup);
            _items.Add(item.Definition.ID, item);
        }

        internal void AddJoin(JoinDrawing join)
        {
            var from = getConnector(join.Definition.From);
            var to = getConnector(join.Definition.To);

            join.PointPath=new []{new Point(),new Point()};

            FollowRelativePosition.Attach(from, this, (p) =>
            {
                //this is only workaround, until there will be path finding algorithm
                var path = new []{p}.Union(join.PointPath.Skip(1)).ToArray();

                join.PointPath=path;
            });

            FollowRelativePosition.Attach(to,this, (p) =>
            {
                //this is only workaround, until there will be path finding algorithm
                var path = new[] { join.PointPath.First(), p };

                join.PointPath = path;
            });

            _output.AddJoin(join);
        }

        #endregion

        #region Services for item states discovering

        internal void SetPosition(FrameworkElement item, Point position)
        {
            DiagramCanvas.SetPosition(item, position);
        }

        internal Point GetPosition(FrameworkElement item)
        {
            return DiagramCanvas.GetPosition(item);
        }
        
        internal DiagramItem GetItem(DrawingReference drawingReference)
        {
            return _items[drawingReference.DefinitionID];
        }

        #endregion

        #region Private utilites

        private ConnectorDrawing getConnector(ConnectorDefinition joinPointDefinition)
        {
            var item = _items[joinPointDefinition.Reference.DefinitionID];
            return item.GetConnector(joinPointDefinition);
        }

        #endregion
    }
}
