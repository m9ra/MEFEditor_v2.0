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
            var fromP = from.GlobalConnectPoint;
            var toP = to.GlobalConnectPoint;

            fromP.Y -= 40;
            toP.Y -= 40;

            var path = getPath(fromP, toP, to.OwningItem, 0);
            return path;
        }

        private IEnumerable<Point> getPath(Point from, Point to, DiagramItem target, int depth)
        {
            if (depth > 20)
                return new[] { from, to };

            var obstacle = _navigator.GetFirstObstacle(from, to);

            if (obstacle == null || obstacle == target)
                //path is clear
                return new[] { from, to };

            var avoidPath = getBestAvoidCorner(from, to, obstacle);


            var incomingPath = getPath(from, avoidPath[0], obstacle, depth + 1);
            var outcomingPath = getPath(avoidPath[1], to, target, depth + 1);

            if (avoidPath[0] == avoidPath[1])
                outcomingPath = outcomingPath.Skip(1);

            return incomingPath.Concat(outcomingPath);
        }

        private Point[] getBestAvoidCorner(Point from, Point to, DiagramItem obstacle)
        {
            var fromVisible = _navigator.GetVisibleCorners(from, obstacle);
            var toVisible = _navigator.GetVisibleCorners(to, obstacle);

            var bothVisible = fromVisible.Intersect(toVisible);


            Point toCorner;
            Point fromCorner;
            if (bothVisible.Any())
            {
                fromCorner = toCorner = bothVisible.First();
            }
            else
            {
                toCorner = _navigator.GetNearest(to, toVisible);
                fromCorner = _navigator.GetNearest(to, fromVisible);
            }

            return new[] { fromCorner, toCorner };
        }


    }
}
