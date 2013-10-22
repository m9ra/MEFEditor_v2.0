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
                var points = PointPath.ToArray();


                var arrGeom = new StreamGeometry();
                using (var context = arrGeom.Open())
                {
                    if (points.Length > 0)
                    {
                        drawJoin(points, context);
                    }
                    context.Close();


                }

                arrGeom.Freeze();

                return arrGeom;
            }
        }

        private static void drawJoin(Point[] points, StreamGeometryContext context)
        {
            context.BeginFigure(points[0], true, false);

            var from = new Point();
            var to = new Point();

            var spline = new BezierSpline(points);

            for (int i = 0; i < spline.FirstControlPoints.Length; ++i)
            {
                from = spline.Knots[i];
                to = spline.Knots[i + 1];

/*                var controlPoint1 = spline.FirstControlPoints[i];
                var controlPoint2 = spline.SecondControlPoints[i];
                
                context.BezierTo(controlPoint1, controlPoint2, to, true, true);*/
                context.LineTo(to,true,true);
            }

            Point arm1;
            Point arm2;
            computeArms(ref from, ref to, out arm1, out arm2);

            context.LineTo(arm1, true, true);
            context.LineTo(to, false, false);
            context.LineTo(arm2, true, true);
        }

        private static void computeArms(ref Point from, ref Point to, out Point arm1, out Point arm2)
        {
            var extraAng = Math.Atan2(to.Y - from.Y, to.X - from.X);

            var armLen = 15;
            var armAngle = 35.0 / 180 * Math.PI;
            var armHalfAn = armAngle / 2;

            arm1 = new Point();
            arm2 = new Point();
            arm1.X = -Math.Cos(armHalfAn + extraAng) * armLen + to.X;
            arm1.Y = -Math.Sin(armHalfAn + extraAng) * armLen + to.Y;
            arm2.X = -Math.Cos(-armHalfAn + extraAng) * armLen + to.X;
            arm2.Y = -Math.Sin(-armHalfAn + extraAng) * armLen + to.Y;
        }
    }
}
