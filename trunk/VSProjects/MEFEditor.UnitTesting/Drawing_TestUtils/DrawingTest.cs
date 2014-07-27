using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MEFEditor.Drawing;
using MEFEditor.Drawing.ArrangeEngine;

using MEFEditor.TypeSystem;

namespace MEFEditor.UnitTesting.Drawing_TestUtils
{
    class DrawingTest
    {
        private readonly Dictionary<string, DiagramItemDefinition> _items = new Dictionary<string, DiagramItemDefinition>();
        private readonly Dictionary<string, string> _joins = new Dictionary<string, string>();

        private DiagramContext _context;


        public static DrawingTest Create { get { return new DrawingTest(); } }

        public DrawingTest Item(string name, double x, double y)
        {
            _items.Add(name, createDef(name, x, y));
            return this;
        }

        public DrawingTest Join(string from, string to)
        {
            _joins.Add(from, to);

            return this;
        }

        private DiagramItemDefinition createDef(string name, double x, double y)
        {
            var def = new DiagramItemDefinition(name, "Test");
            def.GlobalPosition = new Point(x, y);
            def.SetProperty("width", "100");
            def.SetProperty("height", "100");

            return def;
        }

        internal DrawingTest AssertIntersection(Point from, Point to, string intersected)
        {
            var context = getContext();
            var navigator = new SceneNavigator(context.Items);

            var obstacle = navigator.GetFirstObstacle(from, to);
            if (obstacle == null)
            {
                Assert.IsNull(intersected, "Expected obstacle, but scene navigator hasn't detected any");
            }

            Assert.AreEqual<DiagramItemDefinition>(obstacle.Definition, _items[intersected], "Incorrect intersection returned");

            return this;
        }

        private DiagramContext getContext()
        {
            if (_context == null)
            {
                var testCanvas = new DiagramCanvas();
                var diagramDef = new DiagramDefinition(null);
                foreach (var item in _items.Values)
                {
                    diagramDef.DrawItem(item);
                }

                var provider = new DrawingProvider(testCanvas, new TestDrawingFactory());
                _context = provider.Display(diagramDef);

                testCanvas.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                testCanvas.Arrange(new Rect(new Point(), testCanvas.DesiredSize));

            }

            return _context;
        }
    }
}
