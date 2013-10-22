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
            _navigator = new SceneNavigator(engine);
        }

        internal IEnumerable<Point> GetPath(ConnectorDrawing from, ConnectorDrawing to)
        {
            var fromP = from.GlobalConnectPoint;
            var toP = to.GlobalConnectPoint;

            var path = getPath(fromP, toP, 0);
            return path;
        }

        private IEnumerable<Point> getPath(Point from, Point to, int depth)
        {
            if (depth > 2)
                return new[] { from, to };

            var obstacle = _navigator.GetFirstObstacle(from, to);

            if (obstacle == null)
                //path is clear
                return new[] { from, to };

            var avoidPoint = getBestAvoidCorner(from, to, obstacle);
            if (avoidPoint == to)
                return new[] { from, to };

            var avoidingPath = getPath(from, avoidPoint, depth + 1);
            var completingPath = getPath(avoidPoint, to, depth + 1);

            return avoidingPath.Concat(completingPath.Skip(1));
        }

        private Point getBestAvoidCorner(Point from, Point to, DiagramItem obstacle)
        {
            var fromVisible = _navigator.GetVisibleCorners(from, obstacle);
            var bestCorner = _navigator.GetNearest(to, fromVisible);

            return bestCorner;
        }


    }
}
