using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

namespace Drawing.ArrangeEngine
{

    class SceneNavigator
    {
        private readonly DisplayEngine _engine;

        internal SceneNavigator(DisplayEngine engine)
        {
            _engine = engine;
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

        internal DiagramItem GetFirstObstacle(Point from, Point to)
        {
            //TODO change algorithm to use sorted planes !!


            var intersectionDist = Double.PositiveInfinity;
            DiagramItem intersected = null;
            foreach (var item in _engine.Items)
            {
                var span = getSpan(item);
                if (span.Contains(from) || span.Contains(to))
                    continue;

                var intersection = intersectSpan(from, to, span);
                if (!intersection.HasValue)
                    continue;

                var distance = Distance(from, intersection.Value);
                if (distance < intersectionDist)
                {
                    intersectionDist = distance;
                    intersected = item;
                }
            }

            return intersected;
        }


        /// <summary>
        /// Get obstacle corners that are visible from given point
        /// </summary>
        /// <param name="from"></param>
        /// <param name="obstacle"></param>
        /// <returns></returns>
        internal IEnumerable<Point> GetVisibleCorners(Point from, DiagramItem obstacle)
        {
            var span = getSpan(obstacle);
            var categorized = new Point?[]{
                span.TopLeft,span.TopRight,span.BottomLeft,span.BottomRight
            };



            /*
              //THIS IS INCORRECT - Reimplement  
                insert(from, span.TopLeft, categorized);
                insert(from, span.TopRight, categorized);
                insert(from, span.BottomLeft, categorized);
                insert(from, span.BottomRight, categorized);
             */

            for (int i = 0; i < categorized.Length; ++i)
            {
                var visibleCorner = categorized[i];
                if (visibleCorner.HasValue)
                    yield return visibleCorner.Value;
            }
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
