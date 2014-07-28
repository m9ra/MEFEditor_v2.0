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
    /// <summary>
    /// Represent drawing of join between import and export connectors.
    /// </summary>
    public class CompositionJoin : JoinDrawing
    {
        /// <summary>
        /// The error property name.
        /// Here can be stored error for the join.
        /// </summary>
        public const string ErrorPropertyName = "Error";

        /// <summary>
        /// The warning property name.
        /// Here can be stored warning for the join.
        /// </summary>
        public const string WarningPropertyName = "Warning";


        /// <summary>
        /// Initializes a new instance of the <see cref="CompositionJoin"/> class.
        /// </summary>
        /// <param name="definition">The join definition.</param>
        public CompositionJoin(JoinDefinition definition) :
            base(definition)
        {
            this.AllowDrop = false;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is error join.
        /// </summary>
        /// <value><c>true</c> if this instance is error join; otherwise, <c>false</c>.</value>
        public bool IsErrorJoin
        {
            get
            {
                return isConnectorPropertyDefined(ErrorPropertyName);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is warning join.
        /// </summary>
        /// <value><c>true</c> if this instance is warning join; otherwise, <c>false</c>.</value>
        public bool IsWarningJoin
        {
            get
            {
                return isConnectorPropertyDefined(WarningPropertyName);
            }
        }

        /// <summary>
        /// Determines whether connector has defined specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns><c>true</c> if property is defend; otherwise, <c>false</c>.</returns>
        private bool isConnectorPropertyDefined(string propertyName)
        {
            var fromError = Definition.To.GetPropertyValue(propertyName) != null;
            var toError = Definition.From.GetPropertyValue(propertyName) != null;

            return fromError || toError;
        }


        /// <summary>
        /// Create arrow geometry.
        /// </summary>
        /// <value>The defining geometry.</value>
        protected override Geometry DefiningGeometry
        {
            get
            {
                //select color according to error level
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
                
                //highlighted joins will be stronger
                this.StrokeThickness = IsHighlighted ? 4 : 2;
                
                //draw join to geometry
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

        /// <summary>
        /// Draws the join with given context.
        /// </summary>
        /// <param name="points">The points of join.</param>
        /// <param name="context">The context of geometry.</param>
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

                //context.BezierTo(controlPoint1, controlPoint2, to, true, true);
                context.LineTo(to, true, true);
            }

            Point arm1;
            Point arm2;
            computeArms(ref from, ref to, out arm1, out arm2);

            context.LineTo(arm1, true, true);
            context.LineTo(to, false, false);
            context.LineTo(arm2, true, true);
        }

        /// <summary>
        /// Computes the arms.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="arm1">The arm1.</param>
        /// <param name="arm2">The arm2.</param>
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
