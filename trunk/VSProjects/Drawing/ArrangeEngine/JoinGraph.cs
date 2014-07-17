using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;

using Utilities;

namespace Drawing.ArrangeEngine
{

    /// <summary>
    /// Graph used for join path computations. Path finding works in two phases. Firstly is
    /// required to explore path from source to destination. This process can explore more than one
    /// possible path for join line. In second phase the best path of all available is searched.
    /// </summary>
    public class JoinGraph
    {
        #region View angle singleton definitions

        /// <summary>
        /// Quadrants angle for top left corner
        /// </summary>
        private static readonly QuadrantAngle TopLeftCorner = new QuadrantAngle(true, true, true, false);

        /// <summary>
        /// Quadrants angle for top right corner
        /// </summary>
        private static readonly QuadrantAngle TopRightCorner = new QuadrantAngle(true, true, false, true);

        /// <summary>
        /// Quadrants angle for bottom left corner
        /// </summary>
        private static readonly QuadrantAngle BottomLeftCorner = new QuadrantAngle(false, true, true, true);

        /// <summary>
        /// Quadrants angle for bottom right corner
        /// </summary>
        private static readonly QuadrantAngle BottomRightCorner = new QuadrantAngle(true, false, true, true);

        /// <summary>
        /// Quadrants angle that covers all quadrants
        /// </summary>
        private static readonly QuadrantAngle AllAngle = new QuadrantAngle(true, true, true, true);

        /// <summary>
        /// Quadrants angle that doesnt cover any quadrants
        /// </summary>
        private static readonly QuadrantAngle NoAngle = new QuadrantAngle(false, false, false, false);

        /// <summary>
        /// Quadrants angle for top edge
        /// </summary>
        private static readonly QuadrantAngle TopEdge = new QuadrantAngle(true, true, false, false);

        /// <summary>
        /// Quadrants angle for bottom edge
        /// </summary>
        private static readonly QuadrantAngle BottomEdge = new QuadrantAngle(false, false, true, true);

        /// <summary>
        /// Conus angle for top connectors
        /// </summary>
        private static readonly ConusAngle TopConus = new ConusAngle(true, true);

        /// <summary>
        /// Conus angle for bottom connectors
        /// </summary>
        private static readonly ConusAngle BottomConus = new ConusAngle(true, false);

        /// <summary>
        /// Conus angle for right connectors
        /// </summary>
        private static readonly ConusAngle RightConus = new ConusAngle(false, true);

        /// <summary>
        /// Conus angle for left connectors
        /// </summary>        
        private static readonly ConusAngle LeftConus = new ConusAngle(false, false);

        #endregion

        /// <summary>
        /// Navigator used for checking presence of obstacles between points
        /// </summary>
        private readonly SceneNavigator _navigator;

        /// <summary>
        /// Mapping from diagram item to points that it defines
        /// </summary>
        private readonly MultiDictionary<DiagramItem, GraphPoint> _itemPoints = new MultiDictionary<DiagramItem, GraphPoint>();

        /// <summary>
        /// Mapping from connector to corresponding grap points
        /// </summary>
        private readonly Dictionary<ConnectorDrawing, GraphPoint> _connectorPoints = new Dictionary<ConnectorDrawing, GraphPoint>();

        /// <summary>
        /// Queue used for candidate points that are tested for ability to connect with target component
        /// </summary>
        private readonly Queue<GraphPoint> _fromCandidates = new Queue<GraphPoint>();

        /// <summary>
        /// Set of points that has been already processed during path exploration
        /// </summary>
        private readonly HashSet<GraphPoint> _processed = new HashSet<GraphPoint>();

        internal JoinGraph(SceneNavigator navigator)
        {
            _navigator = navigator;
        }

        #region Public API

        /// <summary>
        /// Connect item into graph
        /// </summary>
        /// <param name="item">Item to be connected</param>
        public void AddItem(DiagramItem item)
        {
            var points = generatePoints(item);

            _itemPoints.Add(item, points);
        }

        /// <summary>
        /// Run exploration of path between given connectors
        /// <remarks>Exploration should be runned before path finding</remarks>
        /// </summary>
        /// <param name="from">Connector from which path is explored</param>
        /// <param name="to">Connector to which path is explored</param>
        public void Explore(ConnectorDrawing from, ConnectorDrawing to)
        {
            //initialize global buffers
            _processed.Clear();
            _fromCandidates.Clear();

            //get point representation of source connector
            var fromPoint = getPoint(from);
            if (fromPoint == null)
                return;

            //all points on desired component are possible targets (they all are connected withself)
            var toItem = to.OwningItem;
            var toCandidates = _itemPoints.Get(toItem);

            //run exploration
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

        /// <summary>
        /// Find path beween from and to connectors. 
        /// <remarks>Path finding is processed on current explored state of graph</remarks>
        /// </summary>
        /// <param name="from">Start of desired path</param>
        /// <param name="to">End of desired path</param>
        /// <returns>Path if available, <c>null</c> otherwise</returns>
        public Point[] FindPath(ConnectorDrawing from, ConnectorDrawing to)
        {
            //get connector representation on graph
            var fromPoint = getPoint(from);
            var toPoint = getPoint(to);

            //if there is no connector representation we
            //cannot construct path
            if (fromPoint == null || toPoint == null)
                return null;

            //store points that has been already reached
            var reachedPoints = new Dictionary<GraphPoint, GraphPoint>();
            reachedPoints.Add(fromPoint, null);

            //queue where are stored relaxed graph nodes
            var toRelax = new Dictionary<GraphPoint, double>();
            toRelax.Add(fromPoint, 0);

            while (toRelax.Count > 0)
            {
                var currentPair = extractMin(toRelax);
                var current = currentPair.Key;
                var currentDistance = currentPair.Value;

                //relax all neigbours of current node
                foreach (var neighbour in current.ExploredNeighbours)
                {
                    if (reachedPoints.ContainsKey(neighbour))
                        continue;

                    var fromCurrentDistance = current.DistanceTo(neighbour);
                    var toNeighbourDistance = currentDistance + fromCurrentDistance;

                    double previousNeighbourDistance;
                    if (!toRelax.TryGetValue(neighbour, out previousNeighbourDistance))
                    {
                        toRelax[neighbour] = previousNeighbourDistance = double.MaxValue;
                    }

                    if (previousNeighbourDistance <= toNeighbourDistance)
                        //we have already a better path
                        continue;

                    if (neighbour == current)
                        throw new NotSupportedException();

                    //relax path
                    reachedPoints[neighbour] = current;
                    toRelax[neighbour] = toNeighbourDistance;
                }
            }

            //get representation of path that has been found
            var reconstructed = reconstructPath(toPoint, reachedPoints);
            var simplified = simplifyPath(reconstructed);

            return simplified;
        }

        private KeyValuePair<GraphPoint, double> extractMin(Dictionary<GraphPoint, double> toRelax)
        {

            var maxDistance = double.MaxValue;
            var selected=new KeyValuePair<GraphPoint,double>();

            foreach (var pair in toRelax)
            {
                if (maxDistance > pair.Value)
                {
                    selected = pair;
                    maxDistance = pair.Value;
                }
            }

            toRelax.Remove(selected.Key);

            return selected;
        }

        /// <summary>
        /// Try to remove points that are not necessary
        /// </summary>
        /// <param name="path">Path that will be simplified</param>
        /// <returns>Simplified path</returns>
        private Point[] simplifyPath(GraphPoint[] path)
        {
            if (path == null)
                return null;

            var simplifyFactor = 3;
            var toSimplify = new List<GraphPoint>(path);

            for (var i = 0; i < toSimplify.Count; ++i)
            {
                var skipStart = toSimplify[i];
                for (var j = Math.Min(i + simplifyFactor, toSimplify.Count); j <= i; --j)
                {
                    var skipEnd = toSimplify[j];

                    DiagramItem obstacle;
                    if (tryAddEdge(skipStart, skipEnd, skipEnd.OwningItem, out obstacle) || obstacle == skipStart.OwningItem)
                    {
                        toSimplify.RemoveRange(i + 1, i - j - 1);
                    }
                }
            }

            return toSimplify.Select((p) => p.Position).ToArray();
        }

        #endregion

        #region Graph exploring

        /// <summary>
        /// Try to enqueue point that is candidate of from points
        /// if it hasn't already been enqueued
        /// </summary>
        /// <param name="fromCandidate">Candidate point</param>
        private void tryEnqueue(GraphPoint fromCandidate)
        {
            if (!_processed.Add(fromCandidate))
                //point is already processed
                return;

            _fromCandidates.Enqueue(fromCandidate);
        }

        /// <summary>
        /// Enqueue edge between from candidate and given obstacle
        /// </summary>
        /// <param name="fromCandidate">Candidate from which edge is enqueued</param>
        /// <param name="obstacle">Obstacle which will be connected</param>
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

        /// <summary>
        /// Try to add edge between given points if possible
        /// </summary>
        /// <param name="fromCandidate">Point where edge should start</param>
        /// <param name="toCandidate">Point where edge should end</param>
        /// <param name="target">Target that is not considered to be an obstacle</param>
        /// <param name="obstacle">Obstacle if any is present between from and to candidate, <c>null</c> otherwise</param>
        /// <returns><c>true</c> if edge can be added, <c>false</c> otherwise</returns>
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
            fromCandidate.SetEdgeStatus(toCandidate, isEdgeValid);

            return isEdgeValid;
        }

        #endregion

        #region Path finding

        private static GraphPoint[] reconstructPath(GraphPoint toPoint, Dictionary<GraphPoint, GraphPoint> reachedPoints)
        {
            GraphPoint currentPoint;
            if (!reachedPoints.TryGetValue(toPoint, out currentPoint))
                //path hasn't been found in current graph
                return null;

            var path = new List<GraphPoint>();
            path.Add(toPoint);

            //trace path back from reached table
            while (currentPoint != null)
            {
                path.Add(currentPoint);

                currentPoint = reachedPoints[currentPoint];
            }

            path.Reverse();
            return path.ToArray();
        }

        #endregion

        #region Private utilities

        /// <summary>
        /// Get contact points defined for given item
        /// <remarks>Note that because of simple getting contact points we store them as first four</remarks>
        /// </summary>
        /// <param name="item">Item which points are generated</param>
        /// <returns>Generated points</returns>
        private IEnumerable<GraphPoint> generatePoints(DiagramItem item)
        {
            var span = SceneNavigator.GetSpan(item, item.GlobalPosition);
            var topLeft = new GraphPoint(span.TopLeft, TopLeftCorner, item);
            var topRight = new GraphPoint(span.TopRight, TopRightCorner, item);

            var bottomLeft = new GraphPoint(span.BottomLeft, BottomLeftCorner, item);
            var bottomRight = new GraphPoint(span.BottomRight, BottomRightCorner, item);

            topLeft.SetEdgeStatus(topRight);
            topLeft.SetEdgeStatus(bottomLeft);

            bottomRight.SetEdgeStatus(topRight);
            bottomRight.SetEdgeStatus(bottomLeft);

            var points = new List<GraphPoint>();

            points.Add(topLeft);
            points.Add(topRight);
            points.Add(bottomLeft);
            points.Add(bottomRight);

            generateConnectorPoints(item.TopConnectorDrawings, ConnectorAlign.Top, item, points);
            generateConnectorPoints(item.LeftConnectorDrawings, ConnectorAlign.Left, item, points);
            generateConnectorPoints(item.BottomConnectorDrawings, ConnectorAlign.Bottom, item, points);
            generateConnectorPoints(item.RightConnectorDrawings, ConnectorAlign.Right, item, points);

            return points;
        }

        /// <summary>
        /// Generate points for given connectors
        /// </summary>
        /// <param name="connectors">Connectors which points will be generated</param>
        /// <param name="view">View angle of connectors</param>
        /// <param name="inputPoint"></param>
        /// <param name="points"></param>
        private void generateConnectorPoints(IEnumerable<ConnectorDrawing> connectors, ConnectorAlign connectorAlign, DiagramItem item, List<GraphPoint> points)
        {
            //detect conus for connector direction
            ViewAngle view;
            switch (connectorAlign)
            {
                case ConnectorAlign.Bottom:
                    view = BottomEdge;
                    break;
                case ConnectorAlign.Right:
                    view = RightConus;
                    break;
                case ConnectorAlign.Left:
                    view = LeftConus;
                    break;
                case ConnectorAlign.Top:
                    view = TopEdge;
                    break;
                default:
                    throw new NotSupportedException("Connector align " + connectorAlign);
            }

            //create points for connectors
            var connectorsArray = connectors.ToArray();
            var connectorsCount = connectorsArray.Length;

            for (var i = 0; i < connectorsCount; ++i)
            {
                var connector = connectorsArray[i];

                var point = createPoint(connector, view);
                _connectorPoints.Add(connector, point);

                var inputs = getInputSlots(i, connectorsCount, connector, item, points);
                foreach (var input in inputs)
                {
                    point.SetEdgeStatus(input);
                    points.Add(input);
                }

                points.Add(point);
            }
        }


        private GraphPoint[] getInputSlots(int connectorIndex, int connectorsCount, ConnectorDrawing connector, DiagramItem item, List<GraphPoint> contactPoints)
        {
            var connectorAlign = connector.Align;
            var tightItemSpan = new Rect(item.GlobalPosition, item.DesiredSize);

            //find positions of slots for inputs according to connector align
            Point slot1End, slot1Start;
            Point slot2End;

            ViewAngle slot1View;
            ViewAngle slot2View;

            GraphPoint slot1Contact;
            GraphPoint slot2Contact;

            //slots has to be parallel with same length
            switch (connectorAlign)
            {
                case ConnectorAlign.Top:
                    slot1Contact = contactPoints[2];
                    slot2Contact = contactPoints[3];
                    slot1View = TopLeftCorner;
                    slot2View = TopRightCorner;
                    slot1End = tightItemSpan.TopLeft;
                    slot2End = tightItemSpan.TopRight;
                    slot1Start = new Point(slot1End.X, slot1End.Y + item.TopConnectors.DesiredSize.Height);
                    break;

                case ConnectorAlign.Bottom:
                    slot1Contact = contactPoints[0];
                    slot2Contact = contactPoints[1];
                    slot1View = BottomLeftCorner;
                    slot2View = BottomRightCorner;
                    slot1End = tightItemSpan.BottomLeft;
                    slot2End = tightItemSpan.BottomRight;
                    slot1Start = new Point(slot1End.X, slot1End.Y - item.BottomConnectors.DesiredSize.Height);
                    break;

                case ConnectorAlign.Left:
                    slot1Contact = contactPoints[1];
                    slot2Contact = contactPoints[3];
                    slot1View = TopLeftCorner;
                    slot2View = BottomLeftCorner;
                    slot1End = tightItemSpan.TopLeft;
                    slot2End = tightItemSpan.BottomLeft;
                    slot1Start = new Point(slot1End.X + item.LeftConnectors.DesiredSize.Width, slot1End.Y);
                    break;

                case ConnectorAlign.Right:
                    slot1Contact = contactPoints[0];
                    slot2Contact = contactPoints[2];
                    slot1View = TopRightCorner;
                    slot2View = BottomRightCorner;
                    slot1End = tightItemSpan.TopRight;
                    slot2End = tightItemSpan.BottomRight;
                    slot1Start = new Point(slot1End.X - item.RightConnectors.DesiredSize.Width, slot1End.Y);
                    break;

                default:
                    throw new NotSupportedException("Connector align " + connectorAlign);
            }

            var slotVector = (slot1Start - slot1End) / (connectorsCount + 2);

            var slot1 = new GraphPoint(slot1End + slotVector * connectorIndex, slot1View, item);
            var slot2 = new GraphPoint(slot2End + slotVector * (connectorsCount - connectorIndex), slot2View, item);

            slot1.SetEdgeStatus(slot1Contact);
            slot2.SetEdgeStatus(slot2Contact);

            return new[] { slot1, slot2 };
        }

        /// <summary>
        /// Get points that can be used for connecting given item
        /// </summary>
        /// <param name="item">Item which points are requested</param>
        /// <returns>Contact points of given item</returns>
        private IEnumerable<GraphPoint> getContactPoints(DiagramItem item)
        {
            var points = _itemPoints.Get(item);

            return points.Take(4);
        }

        /// <summary>
        /// Get point representing given connector
        /// </summary>
        /// <param name="connector">Connector which point is requested</param>
        /// <returns>Point representing given connector if available, <c>null</c> otherwise</returns>
        private GraphPoint getPoint(ConnectorDrawing connector)
        {
            GraphPoint connectorPoint;
            _connectorPoints.TryGetValue(connector, out connectorPoint);
            return connectorPoint;
        }

        /// <summary>
        /// Create point for given connector with given view angle
        /// </summary>
        /// <param name="connector">Connector which point is created</param>
        /// <param name="view">View angle of created point</param>
        /// <returns>Created point</returns>
        private GraphPoint createPoint(ConnectorDrawing connector, ViewAngle view)
        {
            var position = connector.GlobalConnectPoint;
            return new GraphPoint(position, view, connector.OwningItem);
        }

        #endregion
    }
}
