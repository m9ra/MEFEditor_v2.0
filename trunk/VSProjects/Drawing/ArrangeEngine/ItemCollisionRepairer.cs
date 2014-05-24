using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

using Utilities;

namespace Drawing.ArrangeEngine
{
    /// <summary>
    /// Class providing abilities to arrange positions of contained items so they wont overlap to each other
    /// </summary>
    class ItemCollisionRepairer
    {
        /// <summary>
        /// Navigator used for detecting collisions
        /// </summary>
        private SceneNavigator _navigator;

        private readonly Dictionary<DiagramItem, Point> _nonCommitedPositions = new Dictionary<DiagramItem, Point>();

        internal void Arrange(IEnumerable<DiagramItem> arrangedItems)
        {
            //TODO use zGroups on container
            var sortedItems = arrangedItems.OrderBy((item) => DiagramCanvasBase.GetZIndex(item));

            _navigator = new SceneNavigator();

            foreach (var item in sortedItems)
            {
                var collidingItems = _navigator.GetItemsInCollision(item).ToArray();

                foreach (var colidingItem in collidingItems)
                {
                    //move colliding items
                    moveCollidingItem(item, colidingItem);
                }
                _navigator.AddItem(item);
            }
        }

        private void moveCollidingItem(DiagramItem item, DiagramItem colidingItem)
        {
            var needs = computeNeeds(colidingItem, item);
            var possibilities = computePossibilities(colidingItem);
            var minimalNeed = findMinimalNeed(needs, possibilities);
            if (!recursiveApplyNeed(minimalNeed, colidingItem))
            {
                //we have to try another direction                
                rollbackPositions();

                //notice that one direction is always feasible, because 
                //containers are stretchable at botom/right sides
                recursiveApplyNeed(needs.GetMove(minimalNeed.Inverse.Direction), colidingItem);
            }
            commitPositions();
        }

        /// <summary>
        /// Select minimal need and apply it recursively for all coliding items
        /// <remarks>
        /// Notice that applying same need on all recursive collisions
        /// is enough for state where no collision has been before
        /// </remarks>
        /// </summary>
        /// <param name="need"></param>
        /// <param name="itemToMove"></param>
        private bool recursiveApplyNeed(Move need, DiagramItem itemToMove)
        {
            if (!need.HasStretchableDirection)
            {
                //we have to check if need appling is allowed
                var possibilities = computePossibilities(itemToMove);
                if (!need.IsSatisfiedBy(possibilities.GetMove(need.Direction)))
                {
                    return false;
                }
            }

            var currentPosition = getPosition(itemToMove);
            var newPosition = need.Apply(currentPosition);
            trySetPosition(itemToMove, newPosition);

            var collidingItems = _navigator.GetItemsInCollision(itemToMove).ToArray();
            foreach (var collidingItem in collidingItems)
            {
                //for other coliding items apply same need
                var hasSuccess = recursiveApplyNeed(need, collidingItem);
                if (!hasSuccess)
                    return false;
            }

            return true;
        }

        private static Move findMinimalNeed(ItemMoveability itemNeeds, ItemMoveability possibilities)
        {
            if (itemNeeds == null)
                return null;

            Move minNeed = null;
            foreach (var need in itemNeeds.Moves)
            {
                var possibility = possibilities.GetMove(need.Direction);

                if (!need.IsSatisfiedBy(possibility))
                {
                    continue;
                }

                if (minNeed == null || minNeed.Length > need.Length)
                {
                    minNeed = need;
                }
            }

            if (minNeed != null)
                //we have found min need
                return minNeed;

            //there is no need satisfied by possibilities, we have to use stretching
            foreach (var need in itemNeeds.Moves)
            {
                if (!need.HasStretchableDirection)
                    continue;

                if (minNeed == null || minNeed.Length > need.Length)
                {
                    minNeed = need;
                }
            }

            return minNeed;
        }

        #region Private utilities

        /// <summary>
        /// Compute needs of movedItem to not to overlap currItem
        /// </summary>
        /// <param name="movedItem">Item that will be moved</param>
        /// <param name="currItem">Item that is tested to be not overlapped</param>
        /// <returns>Computed needs</returns>
        private ItemMoveability computeNeeds(DiagramItem movedItem, DiagramItem currItem)
        {
            var movedSpan = _navigator.GetSpan(movedItem);
            var currSpan = _navigator.GetSpan(currItem);

            var intersection = Rect.Intersect(movedSpan, currSpan);
            var debug = intersection.IsEmpty;

            var upNeed = currSpan.Top - movedSpan.Bottom;
            var downNeed = movedSpan.Top - currSpan.Bottom;
            var rightNeed = movedSpan.Left - currSpan.Right;
            var leftNeed = currSpan.Left - movedSpan.Right;

            var epsilon = 1;
            upNeed = Math.Abs(upNeed) + epsilon;
            downNeed = Math.Abs(downNeed) + epsilon;
            rightNeed = Math.Abs(rightNeed) + epsilon;
            leftNeed = Math.Abs(leftNeed) + epsilon;

            return new ItemMoveability(upNeed, downNeed, leftNeed, rightNeed);
        }

        /// <summary>
        /// Compute possibilities of movedItem to move without overlapping any other item
        /// </summary>
        /// <param name="movedItem">Item which possibilities are created</param>
        /// <returns>Computed possibilities</returns>
        private ItemMoveability computePossibilities(DiagramItem movedItem)
        {
            if (movedItem.IsRootItem)
            {
                return new ItemMoveability(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
            }

            var container = movedItem.ContainingDiagramCanvas;

            //we have to check possibilities relative to parent!
            //so using relative position is necessary
            var currentPosition = getPosition(movedItem);
            var currentLocalPosition = movedItem.AsLocalPosition(currentPosition);
            var span = SceneNavigator.GetSpan(movedItem, currentLocalPosition);

            var contMargin = container.Margin;
            var upPossib = span.Top;
            var downPossib = container.DesiredSize.Height - span.Bottom - contMargin.Bottom - contMargin.Top;
            var rightPossib = container.DesiredSize.Width - span.Right - contMargin.Right - contMargin.Left;
            var leftPossib = span.Left;

            return new ItemMoveability(
                upPossib,
                downPossib,
                leftPossib,
                rightPossib
                );
        }

        private void rollbackPositions()
        {
            var toResetItems = _nonCommitedPositions.Keys.ToArray();
            _nonCommitedPositions.Clear();

            foreach (var toResetItem in toResetItems)
            {
                var position = getPosition(toResetItem);
                _navigator.SetPosition(toResetItem, position);
            }
        }

        private void commitPositions()
        {
            foreach (var toCommitPair in _nonCommitedPositions)
            {
                var item = toCommitPair.Key;
                var position = toCommitPair.Value;

                item.GlobalPosition = position;
            }

            _nonCommitedPositions.Clear();
        }

        private Point getPosition(DiagramItem item)
        {
            Point result;
            if (_nonCommitedPositions.TryGetValue(item, out result))
                return result;

            return item.GlobalPosition;
        }

        private void trySetPosition(DiagramItem item, Point position)
        {
            //store position into temporary store
            _nonCommitedPositions[item] = position;
            _navigator.SetPosition(item, position);
        }

        #endregion
    }
}
