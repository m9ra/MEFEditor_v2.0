using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;

namespace Drawing.ArrangeEngine
{
    /// <summary>
    /// Representation of point in <see cref="JoinGraph"/>. Every point can have multiple edges
    /// to another points. Point also define View angle, which determine edges that can be
    /// establieshed.
    /// </summary>
    class GraphPoint
    {
        /// <summary>
        /// Information about edge attempts from current point. Edge can be established (stored value is <c>true</c>),
        /// forbidden (stored value is <c>false</c>) or unexplored (there is no stored value)
        /// </summary>
        private readonly Dictionary<GraphPoint, bool> _edges = new Dictionary<GraphPoint, bool>();

        /// <summary>
        /// Position of graph point
        /// </summary>
        internal readonly Point Position;

        /// <summary>
        /// Angle limiting reachable points from current point
        /// </summary>
        internal readonly ViewAngle View;

        /// <summary>
        /// Neighbours that has already been explored and that are feasible
        /// </summary>
        internal IEnumerable<GraphPoint> ExploredNeighbours
        {
            get
            {
                return _edges.Where((pair) => pair.Value).Select((pair) => pair.Key);
            }
        }

        internal GraphPoint(Point position, ViewAngle view)
        {
            Position = position;
            View = view;
        }

        /// <summary>
        /// Square part of distance from current point to given point
        /// </summary>
        /// <param name="other">Other point which distance is determined</param>
        /// <returns>Square part of distance</returns>
        internal double SquareDistanceTo(GraphPoint other)
        {
            var oPosition = other.Position;
            var xDiff = Position.X - oPosition.X;
            var yDiff = Position.Y - oPosition.Y;
            return xDiff * xDiff + yDiff * yDiff;
        }

        /// <summary>
        /// Determine that current point has given point in view angle
        /// </summary>
        /// <param name="point">Point which presence in view angle is tested</param>
        /// <returns><c>true</c> if point is view angle, <c>false</c> otherwise</returns>
        internal bool IsInAngle(GraphPoint point)
        {
            return View.IsInAngle(Position, point.Position);
        }

        /// <summary>
        /// Determine that edge status for given point is already available
        /// </summary>
        /// <param name="point">Point which defines tested edge</param>
        /// <returns><c>true</c> if edge status is available, <c>false</c> otherwise</returns>
        internal bool HasEdgeStatus(GraphPoint point)
        {
            return _edges.ContainsKey(point);
        }

        /// <summary>
        /// Set status for edge to given point
        /// </summary>
        /// <param name="point">Point which edge status will be set</param>
        /// <param name="status">Status that will be set</param>
        internal void SetEdgeStatus(GraphPoint point, bool status = true)
        {
            _edges[point] = status;
            point._edges[this] = status;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "[GraphPoint]" + Position;
        }
    }

    /// <summary>
    /// Representation of point of join path
    /// </summary>
    struct PathPoint
    {
        /// <summary>
        /// Point that precedes current point on path
        /// </summary>
        internal readonly GraphPoint PreviousPoint;

        /// <summary>
        /// Distance from path start to current point
        /// </summary>
        internal readonly double Distance;

        public PathPoint(GraphPoint previousPoint, double distance)
        {
            PreviousPoint = previousPoint;
            Distance = distance;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "[PathPoint]{" + PreviousPoint + ", " + Distance + "}";
        }
    }
}
