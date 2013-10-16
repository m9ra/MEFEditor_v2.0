using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Media;

using Drawing;

namespace MEFAnalyzers.Drawings
{
    public class ExportJoin : JoinDrawing
    {
        public ExportJoin(JoinDefinition definition) :
            base(definition)
        {
            this.Stroke = Brushes.Orange;
            this.StrokeThickness = 2;
        }

        

        /// <summary>
        /// Create arrow geometry.
        /// </summary>
        protected override Geometry DefiningGeometry
        {
            get
            {
                var from = PointPath.First();
                var to = PointPath.Last();

                var arrGeom = new StreamGeometry();

                var extraAng = Math.Atan2(to.Y - from.Y, to.X - from.X);

                var armLen = 15;
                var armAngle = 35.0 / 180 * Math.PI;
                var armHalfAn = armAngle / 2;

                var arm1 = new Point();
                var arm2 = new Point();
                arm1.X = -Math.Cos(armHalfAn + extraAng) * armLen + to.X;
                arm1.Y = -Math.Sin(armHalfAn + extraAng) * armLen + to.Y;
                arm2.X = -Math.Cos(-armHalfAn + extraAng) * armLen + to.X;
                arm2.Y = -Math.Sin(-armHalfAn + extraAng) * armLen + to.Y;


                using (var context = arrGeom.Open())
                {
                    context.BeginFigure(from, true, false);
                    context.LineTo(to, true, true);
                    context.LineTo(arm1, true, true);
                    context.LineTo(to, false, false);
                    context.LineTo(arm2, true, true);

                    context.Close();
                }

                arrGeom.Freeze();
                return arrGeom;
            }
        }
    }
}
