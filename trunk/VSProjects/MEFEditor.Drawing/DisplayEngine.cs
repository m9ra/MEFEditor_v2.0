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

using MEFEditor.Drawing.Behaviours;
using MEFEditor.Drawing.ArrangeEngine;

namespace MEFEditor.Drawing
{
    /// <summary>
    /// Implements main logic of diagram display, positioning and arranging.
    /// Provides also interaction logic and behaviours attaching.
    /// </summary>
    internal class DisplayEngine
    {
        /// <summary>
        /// Displayed items according to thier references
        /// </summary>
        private readonly MultiDictionary<string, DiagramItem> _items = new MultiDictionary<string, DiagramItem>();

        /// <summary>
        /// Old positions that are remembered for next drawing
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, Point>> _oldPositions = new Dictionary<string, Dictionary<string, Point>>();

        /// <summary>
        /// Items that are displayed directly in root diagram canvas
        /// </summary>
        private readonly List<DiagramItem> _rootItems = new List<DiagramItem>();

        /// <summary>
        /// All displayed joins
        /// </summary>
        private readonly List<JoinDrawing> _joins = new List<JoinDrawing>();

        /// <summary>
        /// Group used for ordering behaviour
        /// </summary>
        private ElementGroup _orderingGroup = new ElementGroup();

        /// <summary>
        /// Cursor used for positioning root items
        /// </summary>
        private PositionCursor _rootCursor;

        /// <summary>
        /// <see cref="DiagramCanvas"/> where output is displayed
        /// </summary>
        internal readonly DiagramCanvas Output;

        /// <summary>
        /// Navigator that is used for item and join collision avoidance
        /// </summary>
        internal SceneNavigator Navigator { get; private set; }

        /// <summary>
        /// Items that are currently displayed
        /// </summary>
        internal IEnumerable<DiagramItem> Items { get { return _items.Values; } }

        /// <summary>
        /// Joins that are currently displayed
        /// </summary>
        internal IEnumerable<JoinDrawing> Joins { get { return _joins; } }

        /// <summary>
        /// Initialize new instance of displaying engine
        /// </summary>
        /// <param name="output">Output where drawings will be displayed</param>
        internal DisplayEngine(DiagramCanvas output)
        {
            Output = output;

            ContentShiftable.Attach(Output);
            ContentZoomable.Attach(Output);
        }

        #region Display control

        /// <summary>
        /// Display all registered root items
        /// </summary>
        internal void Display()
        {
            _rootCursor = null;

            foreach (var item in _rootItems)
            {
                Output.Children.Add(item);
            }
        }

        /// <summary>
        /// Clear all displayed items
        /// </summary>
        internal void Clear()
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
        /// Clear old positions of items. It causes
        /// forgetting of their positions in next display.
        /// </summary>
        internal void ClearOldPositions()
        {
            _oldPositions.Clear();
        }

        /// <summary>
        /// Register item to be displayed after next <see cref="Display"/> call
        /// </summary>
        /// <param name="item">Registered item</param>
        internal void RegisterItem(DiagramItem item)
        {
            //attach items behaviours
            ItemHighlighting.Attach(item);
            ZOrdering.Attach(item, _orderingGroup);
            DragAndDrop.Attach(item, GetPosition, SetPosition);
            UpdateGlobalPosition.Attach(item);

            _items.Add(item.Definition.ID, item);

            if (item.IsRootItem)
                _rootItems.Add(item);
        }

        /// <summary>
        /// Add join into <see cref="Output"/>
        /// </summary>
        /// <param name="join">Added join</param>
        /// <param name="fromItem">Owner of join start connector</param>
        /// <param name="toItem">Owner of join end connector</param>
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

        #region Services for item states manipulation

        /// <summary>
        /// Set position of given item
        /// </summary>
        /// <param name="item">Positioned item</param>
        /// <param name="position">Position of item</param>
        internal void SetPosition(FrameworkElement item, Point position)
        {
            DiagramCanvasBase.SetPosition(item, position);
        }

        /// <summary>
        /// Get position of given item
        /// </summary>
        /// <param name="item">Item which position is returned</param>
        /// <returns>Item's position</returns>
        internal Point GetPosition(FrameworkElement item)
        {
            return DiagramCanvasBase.GetPosition(item);
        }

        /// <summary>
        /// Get <see cref="DiagramItem"/> that are referenced by given <see cref="ConnectorDefinition"/>
        /// </summary>
        /// <param name="connectorDefinition">Definition of connector</param>
        /// <returns>Referenced <see cref="DiagramItem"/> objects</returns>
        internal IEnumerable<DiagramItem> DefiningItems(ConnectorDefinition connectorDefinition)
        {
            return _items.Get(connectorDefinition.Reference.DefinitionID);
        }

        /// <summary>
        /// Hint position of <see cref="DiagramItem"/> so it can be displayed
        /// on hinted position in next redraw.
        /// </summary>
        /// <param name="hintContext">Context item of hint</param>
        /// <param name="hintedItem">Item that's position is hinted</param>
        /// <param name="point">Hinted position</param>
        internal void HintPosition(DiagramItem hintContext, DiagramItem hintedItem, Point point)
        {
            saveOldPosition(hintContext, hintedItem, point);
        }

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

            var borders = getChildrenBorders(children);

            refreshJoinPaths();

            return borders;
        }

        #endregion

        #region Private utilities

        /// <summary>
        /// Get borders where all children can be present
        /// </summary>
        /// <param name="children">Children which borders are computed</param>
        /// <returns>Computed borders</returns>
        private Size getChildrenBorders(IEnumerable<DiagramItem> children)
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
        /// Set initial positions of given children
        /// </summary>
        /// <param name="parent">Parent of children</param>
        /// <param name="container">Container where children are displayed</param>
        /// <param name="children">Children which positions will be initialized</param>
        private void setInitialPositions(DiagramItem parent, DiagramCanvasBase container, IEnumerable<DiagramItem> children)
        {
            var isRoot = parent == null;
            var lastCursor = isRoot ? _rootCursor : parent.PositionCursor;

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
                    parent.PositionCursor = cursor;
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
            var isRootItem = contextItem == null;
            var contextID = isRootItem ? "" : contextItem.ID;

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
