using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

namespace Drawing.ArrangeEngine
{
    class PositionCursor
    {        /// <summary>
        /// Margin used for instance positioning.
        /// </summary>
        static readonly double Margin = SceneNavigator.Margin + 1;

        /// <summary>
        /// Size, which is computed during affect layout
        /// </summary>
        Size _layoutSize;

        /// <summary>
        /// Determine if next children in affect layout will be pasted at botom, or to the left
        /// </summary>
        bool _nextBottom = true;

        /// <summary>
        /// Offset in direction determined by _nextBotom
        /// </summary>
        Point _nextPosition = new Point(Margin, Margin);

        /// <summary>
        /// Position of item computed in affect layout -> can be used for set item position
        /// </summary>
        Point _lastitemPosition;

        private List<DiagramItem> _requireDefaultPosition = new List<DiagramItem>();

        private Dictionary<DiagramItem, Point> _registeredItems = new Dictionary<DiagramItem, Point>();


        internal void RegisterPosition(DiagramItem item, Point position)
        {
            _registeredItems.Add(item, position);
        }

        internal void SetDefault(DiagramItem item)
        {
            if (item.IsRootItem)
            {
                var currentPos = item.GlobalPosition;
                var isTestPosition = currentPos.X != 0 || currentPos.Y != 0;

                if (isTestPosition)
                    //let the item with preseted position
                    return;
            }

            _requireDefaultPosition.Add(item);
        }

        /// <summary>
        /// Compute and set children positions.
        /// <remarks>
        /// Childrens has to be measured before
        /// </remarks>
        /// </summary>
        internal void UpdatePositions(Size lastLayoutSize)
        {
            _layoutSize = lastLayoutSize;

            updateDefaultPositions();
            updateRegisteredPositions();
        }

        private void updateDefaultPositions()
        {
            if (_requireDefaultPosition.Count == 0)
                //there are no items which requires default positions
                return;

            var items = new List<DiagramItem>();
            //items.AddRange(_registeredItems.Keys);
            items.Sort(itemEdgeComparer);

            var defaultItems = new List<DiagramItem>();
            defaultItems.AddRange(_requireDefaultPosition);
            defaultItems.Sort(itemEdgeComparer);

            items.AddRange(defaultItems);
            var largestSpan = getLocalSpan(items[0]);
            _nextBottom = largestSpan.Height < largestSpan.Width;

            //then resolve position for new children
            foreach (var item in items)
            {
                affectLayout(item);

                if (!_registeredItems.ContainsKey(item))
                    //set default position only for items that 
                    //requires default position
                    item.GlobalPosition = _lastitemPosition;
            }

            //Add margin into canvas
            _layoutSize.Width += Margin;
            _layoutSize.Height += Margin;
        }

        private void updateRegisteredPositions()
        {
            foreach (var itemPair in _registeredItems)
            {
                var item = itemPair.Key;
                var position = itemPair.Value;

                item.GlobalPosition = position;
            }
        }

        private Rect getLocalSpan(DiagramItem item)
        {
            return SceneNavigator.GetSpan(item, item.LocalPosition);
        }

        /// <summary>
        /// Compare two items 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private int itemEdgeComparer(DiagramItem x, DiagramItem y)
        {
            var maxX = Math.Max(x.DesiredSize.Width, x.DesiredSize.Height);
            var maxY = Math.Max(y.DesiredSize.Width, y.DesiredSize.Height);
            return maxY.CompareTo(maxX);
        }

        /// <summary>
        /// Apply algorithm for instance positioning.
        /// </summary>
        /// <param name="item">item that will affect layout.</param>
        private void affectLayout(DiagramItem item)
        {
            if (!hasEnoughSpace(item))
            {
                //switch direction;
                if (_nextBottom) _nextPosition = new Point(_layoutSize.Width + Margin, Margin);
                else _nextPosition = new Point(Margin, _layoutSize.Height + Margin);
                _nextBottom = !_nextBottom;
                //enlarge container
                enlargeTo(item);
            }

            shiftNextPosition(item);
        }

        /// <summary>
        /// Enlarge that item has enough space at next position.
        /// </summary>
        /// <param name="item">item according to is _layout size enlarged.</param>
        private void enlargeTo(DiagramItem item)
        {
            var span = getLocalSpan(item);
            var neededWidth = span.Width + _nextPosition.X;
            var neededHeight = span.Height + _nextPosition.Y;
            neededHeight = Math.Max(neededHeight, _layoutSize.Height);
            neededWidth = Math.Max(neededWidth, _layoutSize.Width);
            _layoutSize.Height = neededHeight;
            _layoutSize.Width = neededWidth;

        }

        /// <summary>
        /// Shift _nextPosition according to _nextBottom direction and item.
        /// </summary>
        /// <param name="item">item according to is position shifted.</param>        
        private void shiftNextPosition(DiagramItem item)
        {
            _lastitemPosition = _nextPosition;
            var thSize = item.DesiredSize;
            if (_nextBottom) _nextPosition.X += thSize.Width + Margin;
            else _nextPosition.Y += thSize.Height + Margin;
        }

        /// <summary>
        /// Determine if item can be pasted on _nextPosition.
        /// </summary>
        /// <param name="item">item to be determined.</param>
        /// <returns>Return true, if there is enough space to paste item.</returns>
        private bool hasEnoughSpace(DiagramItem item)
        {
            var thSize = item.DesiredSize;
            var neededWidth = thSize.Width + _nextPosition.X;
            var neededHeight = thSize.Height + _nextPosition.Y;
            return neededWidth <= _layoutSize.Width && neededHeight <= _layoutSize.Height;
        }
    }
}
