using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

namespace Drawing.ArrangeEngine
{
    /// <summary>
    /// Tracing functionality implementation for join lines displayed within diagram
    /// </summary>
    class JoinTracer
    {
        /// <summary>
        /// Engine where is tracer used
        /// </summary>
        private readonly DisplayEngine _engine;

        /// <summary>
        /// Current navigator for registered items
        /// </summary>
        protected SceneNavigator Navigator { get { return _engine.Navigator; } }

        internal JoinTracer(DisplayEngine engine)
        {
            _engine = engine;
        }

        /// <summary>
        /// Get path between given connectors that doesnt cross any diagram item 
        /// according to current <see cref="SceneNavigator"/>
        /// </summary>
        /// <param name="from">Start of path</param>
        /// <param name="to">End of path</param>
        /// <returns>Points that defines bath between connectors</returns>
        internal IEnumerable<Point> GetPath(ConnectorDrawing from, ConnectorDrawing to)
        {
            Navigator.EnsureGraphInitialized();
            var graph = Navigator.Graph;

            graph.Explore(from, to);
            var result = graph.FindPath(from, to);

            if (result == null)
                result = new[] { from.GlobalConnectPoint, to.GlobalConnectPoint };

            return result;

        }

        private Point[] simplifyPath(Point[] path)
        {
            //TODO improve algorithm

            var simplified = new List<Point>(path);
            for (var i = 0; i < simplified.Count; ++i)
            {
                var p1 = simplified[i];
                for (var j = i + 2; j < simplified.Count; ++j)
                {
                    var p2 = simplified[j];
                    var isClear = Navigator.GetFirstObstacle(p1, p2) == null;
                    if (isClear)
                    {
                        simplified.RemoveRange(i + 1, j - i - 1);
                        j = i + 1;
                    }
                }
            }

            return simplified.ToArray();
        }


        private Point[] getPathRec(Point from, Point to)
        {
            var obstacle = Navigator.GetFirstObstacle(from, to);
            if (from == to || obstacle == null)
                return new[] { from, to };


            var nextPoint = to;

            do
            {
                var corners = Navigator.GetVisibleCorners(from, obstacle);
                if (corners.Contains(nextPoint))
                    break;

                foreach (var corner in corners)
                {
                    nextPoint = corner;

                    obstacle = Navigator.GetFirstObstacle(from, nextPoint);
                    if (obstacle == null)
                        break;
                }
            } while (obstacle != null);

            //path from-nextPoint is clear
            var nextPath = getPathRec(nextPoint, to);
            var finalPath = new[] { from }.Concat(nextPath).ToArray();
            return finalPath;
        }

        private IEnumerable<Point> getPath(Point from, Point to, DiagramItem fromObstacle, int depth)
        {
            if (depth > 5)
                return new[] { from, to };


            var obstacle = Navigator.GetFirstObstacle(from, to);
            var corners = Navigator.GetVisibleCorners(from, obstacle);

            var avoidPoint = obstacle == null ? to : Navigator.GetNearest(to, corners);

            var path = new List<Point>();
            path.Add(from);

            if (fromObstacle != null && avoidPoint != from)
            {
                var fromObstacleCorners = Navigator.GetVisibleCorners(avoidPoint, fromObstacle);
                var fromObstacleFlowPoint = Navigator.GetNearest(from, fromObstacleCorners);

                //this is safe (because it leads along item edge)
                path.Add(fromObstacleFlowPoint);
                from = fromObstacleFlowPoint;
            }

            if (obstacle == null && fromObstacle == null)
            {
                path.Add(to);
                return path;
            }

            var incommingPath = getPath(from, avoidPoint, fromObstacle, depth + 1);
            var outcommingPath = getPath(avoidPoint, to, obstacle, depth + 1);

            return path.Concat(incommingPath).Concat(outcommingPath);

        }

        private Point[] getBestAvoidCorner(Point from, Point to, DiagramItem obstacle)
        {
            var fromVisible = Navigator.GetVisibleCorners(from, obstacle);
            var toVisible = Navigator.GetVisibleCorners(to, obstacle);

            var bothVisible = fromVisible.Intersect(toVisible);


            Point toCorner;
            Point fromCorner;
            if (bothVisible.Any() /*&& _navigator.GetFirstObstacle(from,to)==null*/)
            {
                fromCorner = toCorner = bothVisible.First();
            }
            else
            {
                toCorner = Navigator.GetNearest(to, toVisible);
                fromCorner = Navigator.GetNearest(from, toVisible);
            }

            return new[] { fromCorner, toCorner };
        }


    }
}
