using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

using Utilities;

namespace MEFEditor.Drawing.ArrangeEngine
{
    class Planes
    {
        /// <summary>
        /// Stored planes
        /// </summary>
        private readonly SortedList<double, Plane> _planes;

        private readonly Dictionary<DiagramItem, Plane> _itemPlane = new Dictionary<DiagramItem, Plane>();

        /// <summary>
        /// Determina that vertical or horizontal planes are stored
        /// </summary>
        internal readonly bool IsVertical;

        /// <summary>
        /// Determine that planes are sorted accordint to ascending/descing order
        /// </summary>
        internal readonly bool IsIncremental;

        internal Planes(bool isVertical, bool isIncremental)
        {
            IsVertical = isVertical;
            IsIncremental = isIncremental;

            if (IsIncremental)
            {
                _planes = new SortedList<double, Plane>();
            }
            else
            {
                _planes = new SortedList<double, Plane>(new ReverseComparer<double>());
            }
        }

        internal void AddSegment(DiagramItem item, Point p1, Point p2)
        {
            var key = getPlaneCoordinate(p1);

            Plane segmentsPlane;
            if (!_planes.TryGetValue(key, out segmentsPlane))
            {
                segmentsPlane = new Plane(IsVertical, p1);
                _planes.Add(segmentsPlane.Key, segmentsPlane);
            }

            _itemPlane[item] = segmentsPlane;
            segmentsPlane.AddSegment(item, p1, p2);
        }

        internal void RemoveSegment(DiagramItem item)
        {
            var plane = _itemPlane[item];

            _itemPlane.Remove(item);
            plane.RemoveSegment(item);
            if (plane.IsEmpty)
                //last item form plane has been removed
                _planes.Remove(plane.Key);
        }

        internal DiagramItem GetIntersectedItem(Point from, Point to, out Point intersection)
        {
            var start = getStart(from);

            for (var index = start; index < _planes.Count; ++index)
            {

                var plane = _planes.Values[index];

                if (isInFrontOf(to, plane))
                {
                    intersection = default(Point);
                    return null;
                }

                var planePoint = planeIntersection(from, to, plane);
                var item = plane.GetIntersectedItem(planePoint);
                if (item != null)
                {
                    intersection = planePoint;
                    return item;
                }
            }

            intersection = default(Point);
            return null;
        }


        internal IEnumerable<Plane> GetOrthoPlanesBetween(Point from, Point to)
        {
            var start = getStart(from, true);
            var startCoordinate = getPlaneCoordinate(from);
            var endCoordinate = getPlaneCoordinate(to);

            for (var index = start; index < _planes.Count; ++index)
            {
                var plane = _planes.Values[index];

                if (isInFrontOf(to, plane))
                    break;

                yield return plane;
            }
        }


        private Point planeIntersection(Point from, Point to, Plane plane)
        {
            var distance = getPlaneCoordinate(to) - getPlaneCoordinate(from);
            var slope = getOrthoCoordinate(to) - getOrthoCoordinate(from);
            var planeDistance = plane.Key - getPlaneCoordinate(from);

            var orthoCoord = getOrthoCoordinate(from) + planeDistance * slope / distance;

            if (IsVertical)
            {
                return new Point(plane.Key, orthoCoord);
            }
            else
            {
                return new Point(orthoCoord, plane.Key);
            }
        }

        private bool isInFrontOf(Point to, Plane plane, bool canEqual = false)
        {
            var toCoordinate = getPlaneCoordinate(to);
            var planeCoordinate = plane.Key;

            if (canEqual && toCoordinate == planeCoordinate)
                return true;

            if (IsIncremental)
            {
                return toCoordinate < planeCoordinate;
            }
            else
            {
                return toCoordinate > planeCoordinate;
            }
        }

        internal int getStart(Point from, bool canEqual = false)
        {
            //TODO use binary search

            int index;
            for (index = 0; index < _planes.Count; ++index)
            {
                var plane = _planes.Values[index];
                if (isInFrontOf(from, plane, canEqual))
                    break;
            }
            return index;
        }

        /// <summary>
        /// Get appropriate coordinate according to plane orientation
        /// </summary>
        /// <param name="p">Point which coordinate is resolved</param>
        /// <returns>X Coordinate for vertical plane, Y for horizontal</returns>
        private double getPlaneCoordinate(Point p)
        {
            return Plane.GetPlaneCoordinate(IsVertical, p);
        }

        private double getOrthoCoordinate(Point p)
        {
            return Plane.GetPlaneCoordinate(!IsVertical, p);
        }

    }
}
