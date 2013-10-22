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

        private readonly List<DiagramItem> _rootItems = new List<DiagramItem>();

        private readonly Dictionary<JoinDefinition, Line> _joins = new Dictionary<JoinDefinition, Line>();

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
        }

        public void Clear()
        {
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
        }

        internal void AddJoin(JoinDrawing join, DiagramItem fromItem, DiagramItem toItem)
        {
            var from = fromItem.GetConnector(join.Definition.From);
            var to = toItem.GetConnector(join.Definition.To);
            join.From = from;
            join.To = to;
            
            FollowConnectorPosition.Attach(from, this, (p) =>
            {
                RefreshPointPath(join);
            });

            FollowConnectorPosition.Attach(to, this, (p) =>
            {
                RefreshPointPath(join);
            });

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

        internal Point GetGlobalPosition(DiagramItem item)
        {
            return DiagramCanvas.GetGlobalPosition(item);
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
                    CheckBorders(child, container);
                }
                collisionDetector.AddItem(child);
            }

            collisionDetector.Arrange(container);
        }

        internal void RefreshPointPath(JoinDrawing join)
        {
            var tracer = new JoinTracer(this);
            join.PointPath=tracer.GetPath(join.From, join.To);            
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

        
    }
}
