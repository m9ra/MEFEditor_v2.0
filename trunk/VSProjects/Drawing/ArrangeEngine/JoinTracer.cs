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

            var path = getPath(fromP, toP, to.OwningItem, 0);
            return path;
        }

        private IEnumerable<Point> getPath(Point from, Point to, DiagramItem target, int depth)
        {
            if (depth > 2)
                return new[] { from, to };

            var obstacle = _navigator.GetFirstObstacle(from, to);

            if (obstacle == null || obstacle == target)
                //path is clear
                return new[] { from, to };

            var avoidPoint = getBestAvoidCorner(from, to, obstacle);
            if (avoidPoint == to)
                return new[] { from, to };

            var avoidingPath = getPath(from, avoidPoint, obstacle, depth + 1);
            var completingPath = getPath(avoidPoint, to, target, depth + 1);

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
