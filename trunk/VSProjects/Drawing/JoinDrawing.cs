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

        public readonly JoinDefinition Definition;

        internal ConnectorDrawing From;

        internal ConnectorDrawing To;

        public IEnumerable<Point> PointPath
        {
            get { return (IEnumerable<Point>)this.GetValue(PointPathProperty); }

            set { this.SetValue(PointPathProperty, value.ToArray()); }
        }

        public JoinDrawing(JoinDefinition definition)
        {
            Definition = definition;
        }

    }
}
