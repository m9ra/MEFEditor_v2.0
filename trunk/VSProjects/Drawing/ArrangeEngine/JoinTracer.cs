using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

namespace Drawing.ArrangeEngine
{
    class JoinTracer
    {
        private readonly SceneNavigator _navigator;

        internal JoinTracer(DisplayEngine engine)
        {
            //TODO scene navigator will be shared by engine
            _navigator = new SceneNavigator(engine.Items);
        }

        internal IEnumerable<Point> GetPath(ConnectorDrawing from, ConnectorDrawing to)
        {
            var fromP = from.OutOfItemPoint;
            var toP = to.OutOfItemPoint;

            var path = getPath(fromP, toP, null, 0).ToArray();
            //var path = getPathRec(fromP, toP);

            /*   path[0].Y += 40;
               path[path.Length - 1].Y += 40;*/

            return simplifyPath(path);
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
                    var isClear = _navigator.GetFirstObstacle(p1, p2) == null;
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
            var obstacle = _navigator.GetFirstObstacle(from, to);
            if (from == to || obstacle == null)
                return new[] { from, to };


            var nextPoint = to;

            do
            {
                var corners = _navigator.GetVisibleCorners(from, obstacle);
                if (corners.Contains(nextPoint))
                    break;

                foreach (var corner in corners)
                {
                    nextPoint = corner;

                    obstacle = _navigator.GetFirstObstacle(from, nextPoint);
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


            var obstacle = _navigator.GetFirstObstacle(from, to);
            var corners = _navigator.GetVisibleCorners(from, obstacle);

            var avoidPoint = obstacle == null ? to : _navigator.GetNearest(to, corners);

            var path = new List<Point>();
            path.Add(from);

            if (fromObstacle != null && avoidPoint != from)
            {
                var fromObstacleCorners = _navigator.GetVisibleCorners(avoidPoint, fromObstacle);
                var fromObstacleFlowPoint = _navigator.GetNearest(from, fromObstacleCorners);

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
            var fromVisible = _navigator.GetVisibleCorners(from, obstacle);
            var toVisible = _navigator.GetVisibleCorners(to, obstacle);

            var bothVisible = fromVisible.Intersect(toVisible);


            Point toCorner;
            Point fromCorner;
            if (bothVisible.Any() /*&& _navigator.GetFirstObstacle(from,to)==null*/)
            {
                fromCorner = toCorner = bothVisible.First();
            }
            else
            {
                toCorner = _navigator.GetNearest(to, toVisible);
                fromCorner = _navigator.GetNearest(from, toVisible);
            }

            return new[] { fromCorner, toCorner };
        }


    }
}
