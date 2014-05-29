using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Drawing
{
    public abstract class JoinDrawing : Shape
    {
        public static readonly DependencyProperty PointPathProperty =
          DependencyProperty.Register("PointPath", typeof(Point[]),
          typeof(JoinDrawing), new FrameworkPropertyMetadata(new Point[0],
          FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty IsHighlightedProperty =
          DependencyProperty.Register("IsHighlighted", typeof(bool),
          typeof(JoinDrawing), new FrameworkPropertyMetadata(false,
          FrameworkPropertyMetadataOptions.AffectsRender));

        public readonly JoinDefinition Definition;

        internal ConnectorDrawing From;

        internal ConnectorDrawing To;

        internal Point[] PointPathArray
        {
            get { return (Point[])this.GetValue(PointPathProperty); }
        }

        public bool IsHighlighted
        {
            get
            {
                return (bool)this.GetValue(IsHighlightedProperty);
            }
            internal set
            {
                this.SetValue(IsHighlightedProperty, value);
            }
        }

        public IEnumerable<Point> PointPath
        {
            get { return (IEnumerable<Point>)this.GetValue(PointPathProperty); }

            set
            {
                if (value == null)
                    value = new[] { From.GlobalConnectPoint, To.GlobalConnectPoint };

                this.SetValue(PointPathProperty, value.ToArray());
            }
        }

        public JoinDrawing(JoinDefinition definition)
        {
            Definition = definition;
        }

    }
}
