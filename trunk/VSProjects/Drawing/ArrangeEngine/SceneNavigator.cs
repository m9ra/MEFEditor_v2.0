﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

namespace Drawing.ArrangeEngine
{

    [Flags]
    enum SpanRelativePosition
    {
        Inside=0,

        Above = 1, Bellow = 2, LeftTo = 4, RightTo = 8,

        LeftAbove = Above | LeftTo,
        RightAbove = Above | RightTo,

        LeftBellow = Bellow | LeftTo,
        RightBellow = Bellow | RightTo
    }

    public class SceneNavigator
    {
        private readonly Planes _topBottom = new Planes(false, true);
        private readonly Planes _bottomUp = new Planes(false, false);
        private readonly Planes _leftRight = new Planes(true, true);
        private readonly Planes _rightLeft = new Planes(true, false);

        public SceneNavigator(IEnumerable<DiagramItem> items)
        {
            foreach (var item in items)
            {
                var span = getSpan(item);
                _topBottom.AddSegment(item, span.TopLeft, span.TopRight);
                _bottomUp.AddSegment(item, span.BottomLeft, span.BottomRight);
                _leftRight.AddSegment(item, span.TopLeft, span.BottomLeft);
                _rightLeft.AddSegment(item, span.TopRight, span.BottomRight);
            }
        }


        /// <summary>
        /// Get coordinate distance between given points
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        internal double Distance(Point p1, Point p2)
        {
            var dX = p1.X - p2.X;
            var dY = p2.Y - p2.Y;

            return Math.Abs(dX) + Math.Abs(dY);
        }

        public DiagramItem GetFirstObstacle(Point from, Point to)
        {
            var verticalPlane = selectPlanes(from.X, to.X, _leftRight, _rightLeft);
            var horizontalPlane = selectPlanes(from.Y, to.Y, _topBottom, _bottomUp);

            Point pHorizontal;
            var horizontalItem = intersectedItem(from, to, verticalPlane, out pHorizontal);
            Point pVertical;
            var verticalItem = intersectedItem(from, to, horizontalPlane, out pVertical);

            if (horizontalItem == null)
            {
                return verticalItem;
            }

            if (verticalItem == null)
            {
                return horizontalItem;
            }

            if (Distance(from, pHorizontal) > Distance(from, pVertical))
                return verticalItem;

            return horizontalItem;
        }

        private DiagramItem intersectedItem(Point from, Point to, Planes planes, out Point intersectionPoint)
        {
            if (planes == null)
            {
                intersectionPoint = default(Point);
                return null;
            }

            var item = planes.GetIntersectedItem(from, to, out intersectionPoint);
            return item;
        }

        private Planes selectPlanes(double p1, double p2, Planes incrementalPlanes, Planes decrementalPlanes)
        {
            switch (Math.Sign(p2 - p1))
            {
                case 1:
                    return incrementalPlanes;
                case -1:
                    return decrementalPlanes;
                default:
                    return null;
            }
        }


        /// <summary>
        /// Get obstacle corners that are visible from given point
        /// </summary>
        /// <param name="point">Point which can see visible corners</param>
        /// <param name="obstacle">Obstacle which corners are resolved to visibility</param>
        /// <returns>Enumertion of visible points</returns>
        internal IEnumerable<Point> GetVisibleCorners(Point point, DiagramItem obstacle)
        {
            var span = getSpan(obstacle);
            var position = GetRelativePosition(point, span);

            #region Corners definitions for obstacle positions

            switch (position)
            {
                case SpanRelativePosition.Inside:
                    //point is inside span
                    yield return span.TopLeft;
                    yield return span.TopRight;
                    yield return span.BottomLeft;
                    yield return span.BottomRight;
                    break;

                case SpanRelativePosition.Above:
                    //point is above obstacle
                    yield return span.TopLeft;
                    yield return span.TopRight;
                    break;

                case SpanRelativePosition.Bellow:
                    //point is bellow obstacle
                    yield return span.BottomLeft;
                    yield return span.BottomRight;
                    break;

                case SpanRelativePosition.LeftTo:
                    //point is left to obstacle
                    yield return span.TopLeft;
                    yield return span.BottomLeft;
                    break;

                case SpanRelativePosition.RightTo:
                    //point is right to obstacle                    
                    yield return span.TopRight;
                    yield return span.BottomRight;
                    break;

                case SpanRelativePosition.LeftAbove:
                    //point is left top to obstacle
                    yield return span.TopLeft;
                    yield return span.TopRight;
                    yield return span.BottomLeft;
                    break;

                case SpanRelativePosition.LeftBellow:
                    //point is left bottom to obstacle
                    yield return span.TopLeft;
                    yield return span.BottomLeft;
                    yield return span.BottomRight;
                    break;
                    
                case SpanRelativePosition.RightAbove:
                    //point is top right to obstacle
                    yield return span.TopLeft;
                    yield return span.TopRight;
                    yield return span.BottomRight;
                    break;

                case SpanRelativePosition.RightBellow:
                    //point is right bellow to obstacle                    
                    yield return span.TopRight;
                    yield return span.BottomLeft;
                    yield return span.BottomRight;
                    break;
                
                default:
                    throw new NotSupportedException("This relative position is not reachable");
            }

            #endregion
        }

        /// <summary>
        /// Get span position relative to given point
        /// </summary>
        /// <param name="point">Relative point</param>
        /// <param name="span">Span to be tested</param>
        /// <returns>Relative position to given point</returns>
        internal SpanRelativePosition GetRelativePosition(Point point, Rect span)
        {
            var position = SpanRelativePosition.Inside;
            if (point.Y < span.Top)
            {
                position |= SpanRelativePosition.Above;
            }
            else if (point.Y > span.Bottom)
            {
                position |= SpanRelativePosition.Bellow;
            }

            if (point.X < span.Left)
            {
                position |= SpanRelativePosition.LeftTo;
            }
            else if (point.X > span.Right)
            {
                position |= SpanRelativePosition.RightTo;
            }

            return position;
        }

        internal Point GetNearest(Point to, IEnumerable<Point> points)
        {
            var nearestDistance = Double.PositiveInfinity;
            var nearest = new Point();
            foreach (var point in points)
            {
                var dist = Distance(to, point);
                if (dist < nearestDistance)
                {
                    nearestDistance = dist;
                    nearest = point;
                }
            }

            return nearest;
        }

        private Point? intersectSpan(Point from, Point to, Rect intersected)
        {
            var i = intersected;
            var possibleIntersections = new[]{
                intersectLine(from,to,i.TopLeft,i.TopRight),
                intersectLine(from,to,i.TopRight,i.BottomRight),
                intersectLine(from,to,i.BottomRight,i.BottomLeft),
                intersectLine(from,to,i.BottomLeft,i.TopLeft),
            };

            var intersections = from possible in possibleIntersections where possible.HasValue select possible.Value;
            if (intersections.Any())
                return GetNearest(from, intersections);

            return null;
        }

        private Point? intersectLine(Point p1, Point p2, Point p3, Point p4)
        {
            var point = new Point();
            var d = (p1.X - p2.X) * (p3.Y - p4.Y) - (p1.Y - p2.Y) * (p3.X - p4.X);
            if (d == 0) return null;

            point.X = ((p3.X - p4.X) * (p1.X * p2.Y - p1.Y * p2.X) - (p1.X - p2.X) * (p3.X * p4.Y - p3.Y * p4.X)) / d;
            point.Y = ((p3.Y - p4.Y) * (p1.X * p2.Y - p1.Y * p2.X) - (p1.Y - p2.Y) * (p3.X * p4.Y - p3.Y * p4.X)) / d;

            //TODO solve float non-stability

            if (Math.Round(point.X) < Math.Round(Math.Min(p1.X, p2.X)) ||
                Math.Round(point.X) > Math.Round(Math.Max(p1.X, p2.X))
                )
                return null;

            if (Math.Round(point.X) < Math.Round(Math.Min(p3.X, p4.X)) ||
                Math.Round(point.X) > Math.Round(Math.Max(p3.X, p4.X)))
                return null;

            return point;
        }

        private void insert(Point from, Point inserted, Point?[] categorized)
        {
            var category = getCategory(from, inserted);
            var categorizedPoint = categorized[category];
            if (categorizedPoint.HasValue)
            {
                var presentDist = Distance(from, categorizedPoint.Value);
                var insertedDist = Distance(from, inserted);

                if (insertedDist >= presentDist)
                    //present point is nearer from point
                    return;
            }
            categorized[category] = inserted;
        }

        private Rect getSpan(DiagramItem item)
        {
            return new Rect(item.GlobalPosition, new Size(item.ActualWidth, item.ActualHeight));
        }

        private int getCategory(Point from, Point relative)
        {
            var cat = 0;
            if (from.X > relative.X)
                cat |= 1;

            if (from.Y > relative.Y)
                cat |= 2;

            return cat;
        }

    }
}