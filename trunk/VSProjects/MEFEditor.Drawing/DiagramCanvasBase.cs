using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;

using MEFEditor.Drawing.Behaviours;


namespace MEFEditor.Drawing
{
    /// <summary>
    /// Base canvas that can be used for displaying <see cref="DiagramItem" />.
    /// </summary>
    public abstract class DiagramCanvasBase : Panel
    {
        /// <summary>
        /// The preview drop strategy used for previewing of drop.
        /// </summary>
        internal static readonly DropStrategyBase PreviewDropStrategy = new PreviewDropStrategy();

        /// <summary>
        /// The drop strategy used for proceeding drop.
        /// </summary>
        internal static readonly DropStrategyBase DropStrategy = new DropStrategy();

        /// <summary>
        /// Gets the owner item.
        /// </summary>
        /// <value>The owner item.</value>
        internal DiagramItem OwnerItem { get; private set; }

        /// <summary>
        /// Gets the diagram context.
        /// </summary>
        /// <value>The diagram context.</value>
        internal DiagramContext DiagramContext { get; private set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramCanvasBase" /> class.
        /// </summary>
        protected DiagramCanvasBase()
        {
            AllowDrop = true;
        }

        #region Position property

        /// <summary>
        /// The position property determine local position of <see cref="DiagramItem"/> within <see cref="DiagramCanvas"/>.
        /// </summary>
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.RegisterAttached("Position", typeof(Point),
            typeof(DiagramCanvasBase), new FrameworkPropertyMetadata(new Point(-1, -1),
            FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        /// <summary>
        /// Sets the local position of element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="position">The position.</param>
        public static void SetPosition(UIElement element, Point position)
        {
            element.SetValue(PositionProperty, position);
        }

        /// <summary>
        /// Gets the local position of element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The position.</returns>
        public static Point GetPosition(UIElement element)
        {
            return (Point)element.GetValue(PositionProperty);
        }


        #endregion

        /// <summary>
        /// Gets the global position of canvas.
        /// </summary>
        /// <value>The global position.</value>
        internal Point GlobalPosition
        {
            get
            {
                var isRootCanvas = OwnerItem == null;

                if (isRootCanvas)
                {
                    return new Point();
                }
                else
                {
                    var parentGlobal = OwnerItem.GlobalPosition;
                    var parentOffset = OwnerItem.TranslatePoint(new Point(-1, -1), this);

                    parentGlobal.X -= parentOffset.X;
                    parentGlobal.Y -= parentOffset.Y;

                    return parentGlobal;
                }
            }
        }
        
        /// <summary>
        /// Sets the item that owns current canvas.
        /// </summary>
        /// <param name="owner">The owner.</param>
        internal void SetOwner(DiagramItem owner)
        {
            OwnerItem = owner;
            SetContext(owner.DiagramContext);
            Children.Clear();
        }

        /// <summary>
        /// Sets the context of diagram.
        /// </summary>
        /// <param name="context">The context.</param>
        internal void SetContext(DiagramContext context)
        {
            DiagramContext = context;
        }

        /// <summary>
        /// Adds the join to the canvas.
        /// </summary>
        /// <param name="join">The join.</param>
        internal void AddJoin(JoinDrawing join)
        {
            Children.Add(join);
        }

        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.DragDrop.DragOver" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.DragEventArgs" /> that contains the event data.</param>
        /// <inheritdoc />
        protected override void OnDragOver(DragEventArgs e)
        {
            PreviewDropStrategy.OnDrop(this, e);
        }

        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.DragDrop.DragEnter" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.DragEventArgs" /> that contains the event data.</param>
        /// <inheritdoc />
        protected override void OnDrop(DragEventArgs e)
        {
            DropStrategy.OnDrop(this, e);
            e.Handled = true;
        }

        #region Layout handling

        /// <summary>
        /// Arranges the override.
        /// </summary>
        /// <param name="arrangeSize">Size of the arrange.</param>
        /// <returns>Size.</returns>
        /// <inheritdoc />
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            //positions has been set during measure
            foreach (FrameworkElement child in Children)
            {
                var position = GetPosition(child);
                child.Arrange(new Rect(position, child.DesiredSize));
            }
            return arrangeSize;
        }

        /// <summary>
        /// Measures the override.
        /// </summary>
        /// <param name="constraint">The constraint.</param>
        /// <returns>Size.</returns>
        /// <inheritdoc />
        protected override Size MeasureOverride(Size constraint)
        {
            foreach (UIElement child in Children)
            {
                //no borders on child size
                child.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            }

            if (DiagramContext == null)
                return new Size();

            var size = DiagramContext.Provider.Engine.ArrangeChildren(OwnerItem, this);
            return size;
        }

        #endregion
    }
}
