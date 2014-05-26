using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;

using Utilities;

namespace Drawing.ArrangeEngine
{

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

        internal double SquareDistanceTo(GraphPoint other)
        {
            var oPosition = other.Position;
            var xDiff = Position.X - oPosition.X;
            var yDiff = Position.Y - oPosition.Y;
            return xDiff * xDiff + yDiff * yDiff;
        }

        internal bool IsInAngle(GraphPoint toCandidate)
        {
            return View.IsInAngle(Position, toCandidate.Position);
        }

        internal bool HasEdgeStatus(GraphPoint toCandidate)
        {
            return _edges.ContainsKey(toCandidate);
        }

        internal void SetEdge(GraphPoint toCandidate, bool isEdgeValid = true)
        {
            _edges[toCandidate] = isEdgeValid;
            toCandidate._edges[this] = isEdgeValid;
        }

        public override string ToString()
        {
            return "[GraphPoint]" + Position;
        }
    }

    abstract class ViewAngle
    {
        /// <summary>
        /// Determine that p1 has p2 in its angle
        /// </summary>
        /// <param name="p1">Point from which angle is measured</param>
        /// <param name="p2">Point to which angle is measured</param>
        /// <returns><c>true</c> if p2 is in p1's angle, <c>false</c> otherwise</returns>
        public abstract bool IsInAngle(Point p1, Point p2);
    }

    /// <summary>
    /// Accept points in specified quadrants
    /// 2|1
    /// ---
    /// 3|4
    /// </summary>
    class QuadrantAngle : ViewAngle
    {
        /// <summary>
        /// Indicator for quadrant 1
        /// </summary>
        private readonly bool _q1;

        /// <summary>
        /// Indicator for quadrant 2
        /// </summary>
        private readonly bool _q2;

        /// <summary>
        /// Indicator for quadrant 3
        /// </summary>
        private readonly bool _q3;

        /// <summary>
        /// Indicator for quadrant 4
        /// </summary>
        private readonly bool _q4;

        internal QuadrantAngle(bool q1, bool q2, bool q3, bool q4)
        {
            _q1 = q1;
            _q2 = q2;
            _q3 = q3;
            _q4 = q4;
        }

        /// <inheritdoc />
        public override bool IsInAngle(Point p1, Point p2)
        {
            var isAtRight = p1.X <= p2.X;
            var isAtTop = p1.Y >= p2.Y;
            var isAtBottom = p1.Y <= p2.Y;

            if (isAtRight)
            {
                return (isAtTop && _q1) || (isAtBottom && _q4);

            }
            else
            {
                return (isAtTop && _q2) || (isAtBottom && _q3);
            }
        }
    }

    class ConusAngle : ViewAngle
    {
        private readonly bool _needVertical;

        private readonly bool _needPositive;

        public ConusAngle(bool needVertical, bool needPositive)
        {
            _needVertical = needVertical;
            _needPositive = needPositive;
        }

        public override bool IsInAngle(Point p1, Point p2)
        {
            double main1, main2;
            double minor1, minor2;
            if (_needVertical)
            {
                main1 = p1.Y;
                main2 = p2.Y;

                minor1 = p1.X;
                minor2 = p2.X;
            }
            else
            {
                main1 = p1.X;
                main2 = p2.X;

                minor1 = p1.Y;
                minor2 = p2.Y;
            }

            var mainDiff = main2 - main1;
            var minorDiff = Math.Abs(minor2 - minor1);

            if (Math.Abs(mainDiff) < minorDiff)
                //outside of conus
                return false;

            var isPositive = mainDiff >= 0;
            var isNegative = mainDiff <= 0;

            //check correct orientation
            return (_needPositive && isPositive) || (!_needPositive && isNegative);
        }
    }

    struct PathPoint
    {
        internal readonly GraphPoint PreviousPoint;

        internal readonly double Distance;

        public PathPoint(GraphPoint previousPoint, double distance)
        {
            PreviousPoint = previousPoint;
            Distance = distance;
        }

        public override string ToString()
        {
            return "[PathPoint]{" + PreviousPoint + ", " + Distance + "}";
        }
    }

    public class JoinGraph
    {
        private static readonly QuadrantAngle TopLeftCorner = new QuadrantAngle(true, true, true, false);

        private static readonly QuadrantAngle TopRightCorner = new QuadrantAngle(true, true, false, true);

        private static readonly QuadrantAngle BottomLeftCorner = new QuadrantAngle(false, true, true, true);

        private static readonly QuadrantAngle BottomRightCorner = new QuadrantAngle(true, false, true, true);

        private static readonly QuadrantAngle AllAngle = new QuadrantAngle(true, true, true, true);

        private static readonly QuadrantAngle NoAngle = new QuadrantAngle(false, false, false, false);

        private static readonly ConusAngle TopConus = new ConusAngle(true, true);

        private static readonly ConusAngle BottomConus = new ConusAngle(true, false);

        private static readonly ConusAngle RightConus = new ConusAngle(false, true);

        private static readonly ConusAngle LeftConus = new ConusAngle(false, false);

        private readonly SceneNavigator _navigator;

        private readonly MultiDictionary<DiagramItem, GraphPoint> _itemPoints = new MultiDictionary<DiagramItem, GraphPoint>();

        private readonly Dictionary<ConnectorDrawing, GraphPoint> _connectorPoints = new Dictionary<ConnectorDrawing, GraphPoint>();

        private readonly Queue<GraphPoint> _fromCandidates = new Queue<GraphPoint>();

        private readonly HashSet<GraphPoint> _processed = new HashSet<GraphPoint>();

        internal JoinGraph(SceneNavigator navigator)
        {
            _navigator = navigator;
        }

        public void AddItem(DiagramItem item)
        {
            var points = generatePoints(item);

            _itemPoints.Add(item, points);
        }

        #region Graph exploring

        public void Explore(ConnectorDrawing from, ConnectorDrawing to)
        {
            _processed.Clear();
            _fromCandidates.Clear();

            var fromPoint = getPoint(from);
            if (fromPoint == null)
                return;

            var toItem = to.OwningItem;
            var toCandidates = _itemPoints.Get(toItem);

            tryEnqueue(fromPoint);
            while (_fromCandidates.Count > 0)
            {
                var fromCandidate = _fromCandidates.Dequeue();
                foreach (var toCandidate in toCandidates)
                {
                    DiagramItem obstacle;
                    tryAddEdge(fromCandidate, toCandidate, toItem, out obstacle);
                    enqueueWithObstacleEdges(fromCandidate, obstacle);
                }

                foreach (var neighbour in fromCandidate.ExploredNeighbours)
                {
                    tryEnqueue(neighbour);
                }
            }
        }

        private void tryEnqueue(GraphPoint point)
        {
            if (!_processed.Add(point))
                //point is already processed
                return;

            _fromCandidates.Enqueue(point);
        }

        private void enqueueWithObstacleEdges(GraphPoint fromCandidate, DiagramItem obstacle)
        {
            if (obstacle == null)
                //there is no obstacle which edges can be enqueued
                return;

            var contactPoints = getContactPoints(obstacle);
            foreach (var contactPoint in contactPoints)
            {
                DiagramItem contactObstacle;
                if (tryAddEdge(fromCandidate, contactPoint, obstacle, out contactObstacle))
                {
                    tryEnqueue(contactPoint);
                }
                else
                {
                    enqueueWithObstacleEdges(fromCandidate, contactObstacle);
                }
            }
        }

        private bool tryAddEdge(GraphPoint fromCandidate, GraphPoint toCandidate, DiagramItem target, out DiagramItem obstacle)
        {
            obstacle = null;
            if (
                !fromCandidate.IsInAngle(toCandidate) ||
                !toCandidate.IsInAngle(fromCandidate)
                )
                //edge is not possible between given points
                return false;

            if (fromCandidate.HasEdgeStatus(toCandidate))
                //edge already exists or has been forbidden earlier
                return false;

            obstacle = _navigator.GetFirstObstacle(fromCandidate.Position, toCandidate.Position);
            var isEdgeValid = obstacle == null || obstacle == target;
            fromCandidate.SetEdge(toCandidate, isEdgeValid);

            return isEdgeValid;
        }

        #endregion

        #region Path finding

        public Point[] FindPath(ConnectorDrawing from, ConnectorDrawing to)
        {
            var fromPoint = getPoint(from);
            var toPoint = getPoint(to);

            if (fromPoint == null || toPoint == null)
                return null;

            var reachedPoints = new Dictionary<GraphPoint, PathPoint>();
            reachedPoints.Add(fromPoint, new PathPoint());

            var workQueue = new Queue<GraphPoint>();
            workQueue.Enqueue(fromPoint);
            while (workQueue.Count > 0)
            {
                var current = workQueue.Dequeue();
                var currentDistance = reachedPoints[current].Distance;

                foreach (var neighbour in current.ExploredNeighbours)
                {
                    var fromCurrentDistance = current.SquareDistanceTo(neighbour);
                    var toNeighbourDistance = currentDistance + fromCurrentDistance;

                    PathPoint neighbourReachPoint;
                    if (reachedPoints.TryGetValue(neighbour, out neighbourReachPoint))
                    {
                        //we have reached current neighbour already
                        if (neighbourReachPoint.Distance <= toNeighbourDistance)
                            //we have already a better path
                            continue;
                    }
                    
                    workQueue.Enqueue(neighbour);
                    reachedPoints[neighbour] = new PathPoint(current, toNeighbourDistance);
                }
            }

            return reconstructPath(toPoint, reachedPoints);
        }

        private static Point[] reconstructPath(GraphPoint toPoint, Dictionary<GraphPoint, PathPoint> reachedPoints)
        {
            PathPoint currentPathPoint;
            if (!reachedPoints.TryGetValue(toPoint, out currentPathPoint))
                //path hasn't been found in current graph
                return null;

            var path = new List<Point>();
            path.Add(toPoint.Position);

            //trace path back from reached table
            while (currentPathPoint.PreviousPoint != null)
            {
                var current = currentPathPoint.PreviousPoint;
                path.Add(current.Position);

                currentPathPoint = reachedPoints[current];
            }

            path.Reverse();
            return path.ToArray();
        }

        #endregion


        /// <summary>
        /// Get contact points defined for given item
        /// <remarks>Note that because of simple getting contact points we store them as first four</remarks>
        /// </summary>
        /// <param name="item">Item which points are generated</param>
        /// <returns>Generated points</returns>
        private IEnumerable<GraphPoint> generatePoints(DiagramItem item)
        {
            var span = SceneNavigator.GetSpan(item, item.GlobalPosition);
            var topLeft = new GraphPoint(span.TopLeft, TopLeftCorner);
            var topRight = new GraphPoint(span.TopRight, TopRightCorner);

            var bottomLeft = new GraphPoint(span.BottomLeft, BottomLeftCorner);
            var bottomRight = new GraphPoint(span.BottomRight, BottomRightCorner);

            topLeft.SetEdge(topRight);
            topLeft.SetEdge(bottomLeft);

            bottomRight.SetEdge(topRight);
            bottomRight.SetEdge(bottomLeft);

            var points = new List<GraphPoint>();

            points.Add(topLeft);
            points.Add(topRight);
            points.Add(bottomLeft);
            points.Add(bottomRight);

            generateConnectorPoints(item.TopConnectorDrawings, TopConus, topLeft, points);
            generateConnectorPoints(item.LeftConnectorDrawings, LeftConus, bottomLeft, points);
            generateConnectorPoints(item.BottomConnectorDrawings, BottomConus, bottomRight, points);
            generateConnectorPoints(item.RightConnectorDrawings, RightConus, topRight, points);

            return points;
        }

        private void generateConnectorPoints(IEnumerable<ConnectorDrawing> connectors, ViewAngle view, GraphPoint inputPoint, List<GraphPoint> points)
        {
            foreach (var connector in connectors)
            {
                var point = createPoint(connector, view);
                _connectorPoints.Add(connector, point);

                inputPoint.SetEdge(point);
                points.Add(point);
            }
        }

        private IEnumerable<GraphPoint> getContactPoints(DiagramItem obstacle)
        {
            var points = _itemPoints.Get(obstacle);

            return points.Take(4);
        }

        private GraphPoint getPoint(ConnectorDrawing connector)
        {
            GraphPoint connectorPoint;
            _connectorPoints.TryGetValue(connector, out connectorPoint);
            return connectorPoint;
        }

        private GraphPoint createPoint(ConnectorDrawing connector, ViewAngle view)
        {
            var position = connector.GlobalConnectPoint;
            return new GraphPoint(position, view);
        }
    }
}
