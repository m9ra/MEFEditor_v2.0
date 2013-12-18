using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

namespace Drawing.ArrangeEngine
{

    class SortedMultiList<TKey, TValue> : SortedList<TKey, List<TValue>>
    {
        public IEnumerable<TValue> MultiValues
        {
            get
            {
                foreach (var values in Values)
                {
                    foreach (var value in values)
                        yield return value;
                }
            }
        }
        public void MultiAdd(TKey key, TValue value)
        {
            List<TValue> values;
            if (!TryGetValue(key, out values))
            {
                values = new List<TValue>();
                this[key] = values;
            }

            values.Add(value);
        }
    }

    class SortedItems : SortedMultiList<int, DiagramItem> { }

    class ItemCollisionRepairer
    {
        private readonly SortedList<int, DiagramItem> _zSorted = new SortedList<int, DiagramItem>();

        internal void AddItem(DiagramItem item)
        {
            var zIndex = DiagramCanvasBase.GetZIndex(item);
            _zSorted.Add(zIndex, item);
        }

        internal void Arrange(DiagramCanvasBase container)
        {
            var ordered = _zSorted.Reverse().ToArray();
            for (int i = 0; i < ordered.Length - 1; ++i)
            {
                var currItem = ordered[i].Value;

                for (int j = i + 1; j < ordered.Length; ++j)
                {
                    var movedItem = ordered[j].Value;
                    var currNeeds = computeNeeds(movedItem, currItem);

                    if (currNeeds != null)
                        applyNeed(currNeeds, movedItem, container);
                }
            }
        }

        private Rect GetSpan(DiagramItem item)
        {
            var position = getPosition(item);
            var rect = new Rect(position.X, position.Y, item.DesiredSize.Width, item.DesiredSize.Height);

            return rect;
        }

        private ItemMoveability computeNeeds(DiagramItem movedItem, DiagramItem currItem)
        {
            var movedSpan = GetSpan(movedItem);
            var currSpan = GetSpan(currItem);

            var intersection = Rect.Intersect(movedSpan, currSpan);

            if (intersection.IsEmpty)
                //there is no need for position repairing
                return null;

            var upNeed = currSpan.Top - movedSpan.Bottom;
            var downNeed = movedSpan.Top - currSpan.Bottom;
            var rightNeed = movedSpan.Left - currSpan.Right;
            var leftNeed = currSpan.Left - movedSpan.Right;

            upNeed = Math.Abs(upNeed);
            downNeed = Math.Abs(downNeed);
            rightNeed = Math.Abs(rightNeed);
            leftNeed = Math.Abs(leftNeed);

            return new ItemMoveability(upNeed, downNeed, leftNeed, rightNeed);
        }

        private void applyNeed(ItemMoveability currNeeds, DiagramItem movedItem, DiagramCanvasBase container)
        {
            var possibs = computePossibilities(movedItem, container);


            var minLength = Double.PositiveInfinity;
            Move? minNeed = null;

            foreach (var need in currNeeds.Moves)
            {
                var currLength = need.Length;
                var possibility = possibs.GetMove(need.Direction);

                if (!need.IsSatisfiedBy(possibility))
                {
                    continue;
                }

                if (minLength > currLength)
                {
                    minLength = currLength;
                    minNeed = need;
                }
            }

            if (!minNeed.HasValue)
                return;

            var currPos = getPosition(movedItem);
            var newPos = minNeed.Value.Apply(currPos);
            setPosition(movedItem, newPos);
        }

        private ItemMoveability computePossibilities(DiagramItem movedItem, DiagramCanvasBase container)
        {
            if (movedItem.IsRootItem)
            {
                return new ItemMoveability(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
            }

            var span = GetSpan(movedItem);

            var upPossib = span.Top;
            var downPossib = container.ActualHeight - span.Bottom;
            var rightPossib = container.ActualWidth - span.Right;
            var leftPossib = span.Left;

            return new ItemMoveability(
                upPossib,
                downPossib,
                leftPossib,
                rightPossib
                );
        }

        private Point getPosition(DiagramItem item)
        {
            return DiagramCanvasBase.GetPosition(item);
        }

        private void setPosition(DiagramItem item, Point position)
        {
            DiagramCanvasBase.SetPosition(item, position);
        }
    }
}
