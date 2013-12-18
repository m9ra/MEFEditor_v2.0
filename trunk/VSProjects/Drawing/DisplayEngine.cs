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
using Drawing.ArrangeEngine;

namespace Drawing
{
    delegate void OnConnectorMove(ConnectorDrawing connector);

    class DisplayEngine
    {
        private readonly MultiDictionary<string, DiagramItem> _items = new MultiDictionary<string, DiagramItem>();

        private readonly Dictionary<string, Dictionary<string, Point>> _oldPositions = new Dictionary<string, Dictionary<string, Point>>();


        private readonly List<DiagramItem> _rootItems = new List<DiagramItem>();

        private readonly List<JoinDrawing> _joins = new List<JoinDrawing>();

        internal readonly DiagramCanvas Output;

        ElementGroup _orderingGroup = new ElementGroup();

        internal IEnumerable<DiagramItem> Items { get { return _items.Values; } }

        internal DisplayEngine(DiagramCanvas output)
        {
            Output = output;
        }

        #region Public API

        public void Display()
        {
            foreach (var item in _rootItems)
            {
                Output.Children.Add(item);
            }
            _oldPositions.Clear();
        }

        public void Clear()
        {
            //keep old positions
            foreach (var item in _items.Values)
            {
                var position = GetPosition(item);
                //keep old parent positions
                setOldPosition(item.ParentItem, item, position);
            }

            _orderingGroup = new ElementGroup();
            _items.Clear();
            _rootItems.Clear();
            Output.Children.Clear();
        }
        
        #endregion

        #region Display building methods

        internal void RegisterItem(DiagramItem item)
        {
            ZOrdering.Attach(item, _orderingGroup);
            DragAndDrop.Attach(item, GetPosition, SetPosition);
            UpdateGlobalPosition.Attach(item);
            _items.Add(item.Definition.ID, item);

            if (item.IsRootItem)
                _rootItems.Add(item);

            if (!_oldPositions.ContainsKey(item.ParentID))
            {
                setDefaultPosition(item);
                return;
            }

            var parentPositions = _oldPositions[item.ParentID];
            if (!parentPositions.ContainsKey(item.ID))
            {
                setDefaultPosition(item);
                return;
            }

            SetPosition(item, parentPositions[item.ID]);
        }

        private void setDefaultPosition(DiagramItem item) {
            if (item.IsRootItem)
            {
                //TODO arrange
                SetPosition(item, new Point(50, 50));
            }
        }

        internal void AddJoin(JoinDrawing join, DiagramItem fromItem, DiagramItem toItem)
        {
            var from = fromItem.GetConnector(join.Definition.From);
            var to = toItem.GetConnector(join.Definition.To);
            join.From = from;
            join.To = to;

            FollowConnectorPosition.Attach(from, this, (p) =>
            {
                RefreshJoinPath(join);
            });

            FollowConnectorPosition.Attach(to, this, (p) =>
            {
                RefreshJoinPath(join);
            });

            _joins.Add(join);
            Output.AddJoin(join);
        }

        #endregion

        #region Services for item states discovering

        internal void SetPosition(FrameworkElement item, Point position)
        {
            DiagramCanvasBase.SetPosition(item, position);
        }

        internal Point GetPosition(FrameworkElement item)
        {
            return DiagramCanvasBase.GetPosition(item);
        }

        /// <summary>
        /// DiagramItem contexts containing drawing for given connector definition
        /// </summary>
        /// <param name="connectorDefinition"></param>
        /// <returns></returns>
        internal IEnumerable<DiagramItem> DefiningItems(ConnectorDefinition connectorDefinition)
        {
            return _items.Get(connectorDefinition.Reference.DefinitionID);
        }

        internal void HintPosition(DiagramItem hintContext, DiagramItem hintedItem, Point point)
        {
            setOldPosition(hintContext, hintedItem, point);
        }

        #endregion
        
        internal void ArrangeChildren(DiagramItem owner, DiagramCanvasBase container)
        {
            var collisionDetector = new ItemCollisionRepairer();

            var isRoot = owner == null;
            var children = isRoot ? _rootItems : owner.Children;
            foreach (var child in children)
            {
                if (!isRoot)
                {
                    //only slots are limited to borders
                    if (container.DesiredSize.Height > 0 || container.DesiredSize.Width> 0)
                    {
                        // check only if container is arranged
                        CheckBorders(child, container);
                    }
                }
                collisionDetector.AddItem(child);
            }

            collisionDetector.Arrange(container);

            /*   foreach (var child in children)
               {
                   if (child.HasGlobalPositionChange)
                   {
                       UpdateCrossedLines(child);
                   }
               }*/

            foreach (var join in _joins)
            {
                //TODO: detect if refresh is necessary

                RefreshJoinPath(join);
            }
        }

        private void UpdateCrossedLines(DiagramItem item)
        {
            foreach (var join in _joins)
            {
                if (join.PointPathArray.Length == 0)
                    continue;

                var from = join.PointPathArray[0];
                for (int i = 1; i < join.PointPathArray.Length; ++i)
                {
                    var to = join.PointPathArray[1];

                    //TODO: check crossing between from-to line
                    from = to;
                }
            }
        }

        internal void RefreshJoinPath(JoinDrawing join)
        {
            var tracer = new JoinTracer(this);
            join.PointPath = tracer.GetPath(join.From, join.To);
        }

        private void CheckBorders(FrameworkElement element, DiagramCanvasBase container)
        {
            var position = GetPosition(element);
            var update = false;

            var actualHeight = container.ActualHeight;
            var actualWidth = container.ActualWidth;

            if (position.X + element.ActualWidth > actualWidth)
            {
                update = true;
                position.X = actualWidth - element.ActualWidth;
            }

            if (position.Y + element.ActualHeight > actualHeight)
            {
                update = true;
                position.Y = actualHeight - element.ActualHeight;
            }

            if (position.X < 0)
            {
                update = true;
                position.X = 0;
            }

            if (position.Y < 0)
            {
                update = true;
                position.Y = 0;
            }

            if (update)
                SetPosition(element, position);
        }

        private void setOldPosition(DiagramItem context, DiagramItem item, Point position)
        {
            var contextID = context == null ? "" : context.ID;

            Dictionary<string, Point> positions;
            if (!_oldPositions.TryGetValue(contextID, out positions))
            {
                positions = new Dictionary<string, Point>();
                _oldPositions[contextID] = positions;
            }

            positions[item.ID]=position;
        }

    }
}
