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
        internal JoinTracer()
        {

        }

        internal IEnumerable<Point> GetPath(ConnectorDrawing from, ConnectorDrawing to)
        {
            var fromP = from.GlobalConnectPoint;
            var toP = to.GlobalConnectPoint;
            var testP = new Point(fromP.X, toP.Y);
            return new[] { fromP , toP };
        }
    }
}
