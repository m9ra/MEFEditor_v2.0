﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

namespace MEFEditor.Drawing.ArrangeEngine
{
    class Plane
    {
        /// <summary>
        /// Start coordinates of stored segments
        /// </summary>
        private readonly List<double> _segmentStarts = new List<double>();

        /// <summary>
        /// End coordinates of stored segments
        /// </summary>
        private readonly List<double> _segmentEnds = new List<double>();

        /// <summary>
        /// Items attached to stored segments
        /// </summary>
        private readonly List<DiagramItem> _attachedItems = new List<DiagramItem>();

        /// <summary>
        /// Coordinate (x, or y, depending on Plane orientation)
        /// </summary>
        internal readonly double Key;

        /// <summary>
        /// Determine that this plane has vertical/horizontal orientation
        /// All segments has to be in same vertical/horizontal layer
        /// </summary>
        internal readonly bool IsVertical;

        /// <summary>
        /// Determine that plane is empty
        /// </summary>
        internal bool IsEmpty { get { return _attachedItems.Count == 0; } }

        internal Plane(bool isVertical, Point key)
        {
            IsVertical = isVertical;
            Key = getPlaneCoordinate(key);
        }


        internal void AddSegment(DiagramItem attachedItem, Point p1, Point p2)
        {
            var c1 = getOrthoCoordinate(p1);
            var c2 = getOrthoCoordinate(p2);

            if (c1 > c2)
            {
                var swp = c2;
                c2 = c1;
                c1 = swp;
            }

            _segmentStarts.Add(c1);
            _segmentEnds.Add(c2);
            _attachedItems.Add(attachedItem);
        }

        internal void RemoveSegment(DiagramItem item)
        {
            var index = _attachedItems.IndexOf(item);
            _segmentEnds.RemoveAt(index);
            _segmentStarts.RemoveAt(index);
            _attachedItems.RemoveAt(index);
        }

        /// <summary>
        /// Get plane defining coordinate according to plane orientation
        /// </summary>
        /// <param name="isVertical">Determine that coordinate should be vertical.</param>
        /// <param name="p">Point which coordinate is resolved</param>
        /// <returns>X Coordinate for vertical plane, Y for horizontal</returns>
        internal static double GetPlaneCoordinate(bool isVertical, Point p)
        {
            return isVertical ? p.X : p.Y;
        }


        /// <summary>
        /// Get plane defining coordinate according to plane orientation
        /// </summary>
        /// <param name="p">Point which coordinate is resolved</param>
        /// <returns>X Coordinate for vertical plane, Y for horizontal</returns>
        private double getPlaneCoordinate(Point p)
        {
            return GetPlaneCoordinate(IsVertical, p);
        }

        private double getOrthoCoordinate(Point p)
        {
            return GetPlaneCoordinate(!IsVertical, p);
        }


        internal DiagramItem GetIntersectedItem(Point planePoint)
        {
            var intersectCoord = getOrthoCoordinate(planePoint);
            for (int i = 0; i < _segmentStarts.Count; ++i)
            {
                var start = _segmentStarts[i];
                if (intersectCoord < start)
                    continue;

                var end = _segmentEnds[i];
                if (intersectCoord > end)
                    continue;

                return _attachedItems[i];
            }
            return null;
        }

        internal IEnumerable<DiagramItem> ItemsBetween(Point from, Point to)
        {
            var start = getOrthoCoordinate(from);
            var end = getOrthoCoordinate(to);

            for (int i = 0; i < _segmentStarts.Count; ++i)
            {
                var segmentStart = _segmentStarts[i];
                var segmentEnd = _segmentEnds[i];
                var item = _attachedItems[i];

                if (
                        (segmentStart >= start && segmentStart <= end) ||   //start inside
                        (segmentEnd >= start && segmentEnd <= end) ||       //end inside

                        (segmentStart <= start && segmentEnd >= end)        //segment outside
                    )
                    yield return item;
            }
        }

        public override string ToString()
        {
            var direction = IsVertical ? "Vertical" : "Horizontal";
            return string.Format("Plane|{0}: {1}", direction, Key);
        }


    }
}
