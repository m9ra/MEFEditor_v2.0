using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Media;

using MEFEditor.Drawing;

namespace RecommendedExtensions.Core.Drawings
{
    public class CompositionJoin : JoinDrawing
    {
        public const string ErrorPropertyName = "Error";

        public const string WarningPropertyName = "Warning";


        public CompositionJoin(JoinDefinition definition) :
            base(definition)
        {
            this.AllowDrop = false;
        }

        public bool IsErrorJoin
        {
            get
            {
                return isConnectorPropertyDefined(ErrorPropertyName);
            }
        }

        public bool IsWarningJoin
        {
            get
            {
                return isConnectorPropertyDefined(WarningPropertyName);
            }
        }

        private bool isConnectorPropertyDefined(string propertyName)
        {
            var fromError = Definition.To.GetPropertyValue(propertyName) != null;
            var toError = Definition.From.GetPropertyValue(propertyName) != null;

            return fromError || toError;
        }


        /// <summary>
        /// Create arrow geometry.
        /// </summary>
        protected override Geometry DefiningGeometry
        {
            get
            {
                if (IsErrorJoin)
                {
                    Stroke = Brushes.Red;
                }
                else if (IsWarningJoin)
                {
                    Stroke = Brushes.Orange;
                }
                else
                {
                    Stroke = Brushes.DimGray;
                }
                
                this.StrokeThickness = IsHighlighted ? 4 : 2;
                
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


            // Get Bezier Spline Control Points.
            Point[] controlPoints1, controlPoints2;
            BezierSpline.GetCurveControlPoints(points, out controlPoints1, out controlPoints2);

            for (var i = 0; i < controlPoints1.Length; ++i)
            {
                from = points[i];
                to = points[i + 1];
                var controlPoint1 = controlPoints1[i];
                var controlPoint2 = controlPoints2[i];

                /*/
                context.BezierTo(controlPoint1, controlPoint2, to, true, true);
                /*/
                context.LineTo(to, true, true);
                /**/
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
