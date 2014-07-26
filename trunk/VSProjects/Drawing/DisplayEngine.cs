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

        private ElementGroup _orderingGroup = new ElementGroup();

        private PositionCursor _rootCursor;

        internal readonly DiagramCanvas Output;

        internal SceneNavigator Navigator { get; private set; }

        internal IEnumerable<DiagramItem> Items { get { return _items.Values; } }

        internal IEnumerable<JoinDrawing> Joins { get { return _joins; } }

        internal DisplayEngine(DiagramCanvas output)
        {
            Output = output;

            ContentShiftable.Attach(Output);
            ContentZoomable.Attach(Output);
        }

        #region Public API

        public void Display()
        {
            _rootCursor = null;

            foreach (var item in _rootItems)
            {
                Output.Children.Add(item);
            }
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
            _joins.Clear();
            _rootItems.Clear();
            Output.Clear();
        }

        #endregion

        #region Display building methods

        /// <summary>
        /// Clear old positions of items
        /// </summary>
        internal void ClearOldPositions()
        {
            _oldPositions.Clear();
        }

        internal void RegisterItem(DiagramItem item)
        {
            ItemHighlighting.Attach(item);
            ZOrdering.Attach(item, _orderingGroup);
            DragAndDrop.Attach(item, GetPosition, SetPosition);
            UpdateGlobalPosition.Attach(item);


            _items.Add(item.Definition.ID, item);

            if (item.IsRootItem)
                _rootItems.Add(item);
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
        internal Size ArrangeChildren(DiagramItem owner, DiagramCanvasBase container)
        {
            var isRoot = owner == null;
            var children = isRoot ? _rootItems : owner.Children;

            setInitialPositions(owner, container, children);

            if (Output.DiagramContext.Diagram.UseItemAvoidance)
            {
                var collisionRepairer = new ItemCollisionRepairer();
                collisionRepairer.Arrange(children);
            }

            //create navigator after items are positioned
            Navigator = new SceneNavigator(Items);

            var borders = getChildrenBorders(container, children);

            refreshJoinPaths();

            return borders;
        }

        private Size getChildrenBorders(DiagramCanvasBase container, IEnumerable<DiagramItem> children)
        {
            var maxX = 0.0;
            var maxY = 0.0;

            foreach (var child in children)
            {
                var span = Navigator.GetSpan(child);
                var position = child.LocalPosition;

                maxX = Math.Max(maxX, position.X + span.Width);
                maxY = Math.Max(maxY, position.Y + span.Height);
            }

            return new Size(maxX, maxY);
        }

        /// <summary>
        /// Recompoute join path for all available joins
        /// </summary>
        private void refreshJoinPaths()
        {
            //TODO: detect if refresh is necessary
            if (Output.DiagramContext.Diagram.UseJoinAvoidance)
            {
                Navigator.EnsureGraphInitialized();
                foreach (var join in _joins)
                {
                    Navigator.Graph.Explore(join.From, join.To);
                }

                foreach (var join in _joins)
                {
                    join.PointPath = Navigator.Graph.FindPath(join.From, join.To);
                }
            }
            else
            {
                foreach (var join in _joins)
                {
                    join.PointPath = new[] { join.From.GlobalConnectPoint, join.To.GlobalConnectPoint };
                }
            }
        }


        /// <summary>
        /// Recompoute join path for given join
        /// </summary>
        /// <param name="join">Join which path will be recomputed</param>
        private void refreshJoinPath(JoinDrawing join)
        {
            //TODO avoid uneccessary path finding
            Navigator = new SceneNavigator(Items);
            Navigator.EnsureGraphInitialized();

            if (Output.DiagramContext.Diagram.UseJoinAvoidance)
            {
                Navigator.Graph.Explore(join.From, join.To);
                join.PointPath = Navigator.Graph.FindPath(join.From, join.To);
            }
            else
            {
                join.PointPath = new[] { join.From.GlobalConnectPoint, join.To.GlobalConnectPoint };
            }
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


            var contMargin = container.Margin;

            var containerHeight = container.DesiredSize.Height - contMargin.Bottom - contMargin.Top;
            var containerWidth = container.DesiredSize.Width - contMargin.Left - contMargin.Right;

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

        private void setInitialPositions(DiagramItem owner, DiagramCanvasBase container, IEnumerable<DiagramItem> children)
        {
            var isRoot = owner == null;
            var lastCursor = isRoot ? _rootCursor : owner.PositionCursor;

            var needsInitialPositions = lastCursor == null;
            if (needsInitialPositions)
            {
                var cursor = new PositionCursor();
                Size lastLayoutSize;

                if (isRoot)
                {
                    //root has no owner, because of that has special cursor
                    _rootCursor = cursor;
                    //we dont want to limit size of root canvas
                    lastLayoutSize = new Size();
                }
                else
                {
                    //owner should keep their cursors
                    owner.PositionCursor = cursor;
                    lastLayoutSize = container.DesiredSize;
                }

                //inform cursor about items
                foreach (var child in children)
                {
                    setInitialPosition(cursor, child);
                }

                //update positions of children
                cursor.UpdatePositions(lastLayoutSize);
            }

            foreach (var child in children)
            {
                child.GlobalPosition = checkBounds(child, child.GlobalPosition);
            }
        }

        /// <summary>
        /// Set initial position for given item. 
        /// Accordingly old positions, default positions,..
        /// </summary>
        /// <param name="item">Item which position will be set</param>
        private void setInitialPosition(PositionCursor cursor, DiagramItem item)
        {
            Point oldPosition;
            if (item.HasPosition)
            {
                //keep preset position
                oldPosition = item.GlobalPosition;
            }
            else
            {
                if (!_oldPositions.ContainsKey(item.ParentID))
                {
                    //there is no old position for item
                    cursor.SetDefault(item);
                    return;
                }

                var parentPositions = _oldPositions[item.ParentID];
                if (!parentPositions.ContainsKey(item.ID))
                {
                    //there is no old position for item
                    cursor.SetDefault(item);
                    return;
                }

                //keep position from previous display
                oldPosition = parentPositions[item.ID];
            }

            cursor.RegisterPosition(item, oldPosition);
        }

        /// <summary>
        /// Repair position
        /// </summary>
        /// <param name="globalPosition">Position to repair.</param>
        /// <returns>Repaired position.</returns>
        private Point checkBounds(DiagramItem item, Point globalPosition)
        {
            if (item.IsRootItem)
                return globalPosition;

            var localPosition = item.AsLocalPosition(globalPosition);

            var left = globalPosition.X;
            var top = globalPosition.Y;

            if (localPosition.X < 0) left -= localPosition.X;
            if (localPosition.Y < 0) top -= localPosition.Y;

            return new Point(left, top);
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
