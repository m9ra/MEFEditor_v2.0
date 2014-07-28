using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MEFEditor.Drawing
{
    /// <summary>
    /// Abstract class that defines implementation of joins displayed on
    /// <see cref="DiagramCanvas" />. Join is defined by <see cref="PointPathProperty" />
    /// which contains array of points which should be followed by join.
    /// </summary>
    public abstract class JoinDrawing : Shape
    {
        /// <summary>
        /// The point path property.
        /// </summary>
        public static readonly DependencyProperty PointPathProperty =
          DependencyProperty.Register("PointPath", typeof(Point[]),
          typeof(JoinDrawing), new FrameworkPropertyMetadata(new Point[0],
          FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// The is highlighted property.
        /// </summary>
        public static readonly DependencyProperty IsHighlightedProperty =
          DependencyProperty.Register("IsHighlighted", typeof(bool),
          typeof(JoinDrawing), new FrameworkPropertyMetadata(false,
          FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// The definition of join drawing.
        /// </summary>
        public readonly JoinDefinition Definition;

        /// <summary>
        /// Source connector of join.
        /// </summary>
        internal ConnectorDrawing From;

        /// <summary>
        /// Target connector of join.
        /// </summary>
        internal ConnectorDrawing To;

        /// <summary>
        /// Gets the point path array.
        /// </summary>
        /// <value>The point path array.</value>
        internal Point[] PointPathArray
        {
            get { return (Point[])this.GetValue(PointPathProperty); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is highlighted.
        /// </summary>
        /// <value><c>true</c> if this instance is highlighted; otherwise, <c>false</c>.</value>
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

        /// <summary>
        /// Gets or sets the point path. Points on this path
        /// has to be followed by current join.
        /// </summary>
        /// <value>The point path.</value>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="JoinDrawing" /> class.
        /// </summary>
        /// <param name="definition">The join drawing definition.</param>
        public JoinDrawing(JoinDefinition definition)
        {
            Definition = definition;
        }

    }
}
