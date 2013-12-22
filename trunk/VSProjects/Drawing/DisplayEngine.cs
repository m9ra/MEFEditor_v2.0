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

            ContentShiftable.Attach(Output);
            ContentZoomable.Attach(Output);
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
                saveOldPosition(item.ParentItem, item, position);
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

            setInitialPosition(item);
        }
            
        internal void AddJoin(JoinDrawing join, DiagramItem fromItem, DiagramItem toItem)
        {
            var from = fromItem.GetConnector(join.Definition.From);
            var to = toItem.GetConnector(join.Definition.To);
            join.From = from;
            join.To = to;

            FollowConnectorPosition.Attach(from, this, (p) =>
            {
                refreshJoinPath(join);
            });

            FollowConnectorPosition.Attach(to, this, (p) =>
            {
                refreshJoinPath(join);
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
            saveOldPosition(hintContext, hintedItem, point);
        }

        #endregion
        
        /// <summary>
        /// Arrange children of given owner according to Arrange algorithm
        /// </summary>
        /// <param name="owner">Owner which children will be arranged</param>
        /// <param name="container">Container where children are arranged</param>
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
                    if (container.DesiredSize.Height > 0 || container.DesiredSize.Width > 0)
                    {
                        // check only if container is arranged
                        checkBorders(child, container);
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

                refreshJoinPath(join);
            }
        }

        /// <summary>
        /// Recompoute join path for given join
        /// </summary>
        /// <param name="join">Join which path will be recomputed</param>
        private void refreshJoinPath(JoinDrawing join)
        {
            var tracer = new JoinTracer(this);
            join.PointPath = tracer.GetPath(join.From, join.To);
        }

        /// <summary>
        /// Check position according to borders of given element within given container
        /// </summary>
        /// <param name="element">Element which position is checked</param>
        /// <param name="container">Container which borders are used for position check</param>
        private void checkBorders(FrameworkElement element, DiagramCanvasBase container)
        {
            var position = GetPosition(element);
            var update = false;

            var containerHeight = container.DesiredSize.Height;
            var containerWidth = container.DesiredSize.Width;

            var elHeight = element.DesiredSize.Height;
            var elWidth = element.DesiredSize.Width;


            if (position.X + elWidth > containerWidth)
            {
                update = true;
                position.X = containerWidth - elWidth;
            }

            if (position.Y + elHeight > containerHeight)
            {
                update = true;
                position.Y = containerHeight - elHeight;
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

        private void updateCrossedLines(DiagramItem item)
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
        
        #region Positioning routines

        /// <summary>
        /// Set initial position for given item. 
        /// Accordingly old positions, default positions,..
        /// </summary>
        /// <param name="item">Item which position will be set</param>
        private void setInitialPosition(DiagramItem item)
        {
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

        /// <summary>
        /// Set default position for given item
        /// </summary>
        /// <param name="item">Item which position will be set</param>
        private void setDefaultPosition(DiagramItem item)
        {
            //TODO refactor
            if (item.IsRootItem)
            {
                //TODO arrange items
                var currentPos = item.GlobalPosition;

                if (currentPos.X == 0 && currentPos.Y == 0)
                {
                    SetPosition(item, new Point(50, 50));
                }
                else
                {
                    //needed because of tests
                    SetPosition(item, currentPos);
                }
            }
        }

        /// <summary>
        /// Save position of given item to be hold for another display
        /// </summary>
        /// <param name="contextItem">Context item, where saved position is valid</param>
        /// <param name="item">Item which position is saved</param>
        /// <param name="position">Saved position</param>
        private void saveOldPosition(DiagramItem contextItem, DiagramItem item, Point position)
        {
            var contextID = contextItem == null ? "" : contextItem.ID;

            Dictionary<string, Point> positions;
            if (!_oldPositions.TryGetValue(contextID, out positions))
            {
                positions = new Dictionary<string, Point>();
                _oldPositions[contextID] = positions;
            }

            positions[item.ID] = position;
        }

        #endregion
    }
}
