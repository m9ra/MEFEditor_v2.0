using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;

using Drawing.Behaviours;


namespace Drawing
{
    public abstract class DiagramCanvasBase : Panel
    {
        private DiagramItem _ownerItem;

        private DiagramContext _context;


        protected DiagramCanvasBase()
        {
            AllowDrop = true;
        }

        #region Position property

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.RegisterAttached("Position", typeof(Point),
            typeof(DiagramCanvasBase), new FrameworkPropertyMetadata(new Point(-1, -1),
            FrameworkPropertyMetadataOptions.AffectsParentArrange));

        public static void SetPosition(UIElement element, Point position)
        {
            element.SetValue(PositionProperty, position);
        }

        public static Point GetPosition(UIElement element)
        {
            return (Point)element.GetValue(PositionProperty);
        }


        #endregion

        internal Point GlobalPosition
        {
            get
            {
                var isRootCanvas = _ownerItem == null;

                if (isRootCanvas)
                {
                    return new Point();
                }
                else
                {
                    var parentGlobal = _ownerItem.GlobalPosition;
                    var parentOffset = _ownerItem.TranslatePoint(new Point(0, 0), this);

                    parentGlobal.X -= parentOffset.X;
                    parentGlobal.Y -= parentOffset.Y;

                    return parentGlobal;
                }
            }
        }


        internal void SetOwner(DiagramItem owner)
        {
            _ownerItem = owner;
            SetContext(owner.DiagramContext);
            Children.Clear();
        }

        internal void SetContext(DiagramContext context)
        {
            _context = context;
        }

        internal void AddJoin(JoinDrawing join)
        {
            Children.Add(join);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (_context != null)
            {
                _context.Provider.Engine.ArrangeChildren(_ownerItem, this);
            }

            foreach (FrameworkElement child in Children)
            {
                var position = GetPosition(child);

                child.Arrange(new Rect(position, child.DesiredSize));

                var item = child as DiagramItem;
                if (item == null)
                    continue;

                //item.RefreshGlobal();
            }
            return arrangeSize;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            foreach (UIElement child in Children)
            {
                //no borders on child size
                child.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            }

            //canvas doesn't need no size itself
            return new Size(MinHeight, MinWidth);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            var dragAdorner = e.Data.GetData("DragAdorner") as DragAdorner;
            if (dragAdorner == null)
                return;

            e.Handled = true;

            var dragItem = dragAdorner.Item;

            if (dragItem.ContainingDiagramCanvas == this || dragItem.IsRootItem)
            {
                dragAdorner.Hint = "Change item position";
                e.Effects = DragDropEffects.Move;
                return;
            }

            string hint;
            if (dragItem.CanExcludeFromParent)
            {
                hint = string.Format("Exclude from: '{0}'", dragItem.ParentItem.ID);

                if (_ownerItem != null)
                {
                    throw new NotImplementedException("Determine that parent can accept");
                    //hint += string.Format("\nAccept to '{0}'", _ownerItem.ID);
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
                hint = string.Format("Can't exclude from: '{0}'", dragItem.ParentItem.ID);
            }
            dragAdorner.Hint = hint;
        }

        protected override void OnDrop(DragEventArgs e)
        {
            var dragAdorner = e.Data.GetData("DragAdorner") as DragAdorner;
            if (dragAdorner == null)
                return;

            e.Handled = true;

            var dragItem = dragAdorner.Item;
            dragItem.GlobalPosition = dragAdorner.GlobalPosition;

            if (_ownerItem == dragItem)
                //cant move self to sub slot
                return;

            if (dragItem.ContainingDiagramCanvas == this)
                //move within this canvas
                return;

            excludeFromParent(dragItem);
            acceptItem(dragAdorner);
        }

        private void excludeFromParent(DiagramItem dragItem)
        {
            if (dragItem.IsRootItem)
                //no drop action
                return;

            if (dragItem.CanExcludeFromParent)
            {
                var diff = dragItem.GlobalPosition - GlobalPosition;
                _context.HintPosition(_ownerItem, dragItem, new Point(diff.X, diff.Y));
                dragItem.ParentExcludeEdit.Action();
            }
        }

        private void acceptItem(DragAdorner dragAdorner)
        {
            var dragItem = dragAdorner.Item;

            if (dragItem.ContainingDiagramCanvas == this)
            {
                //item moving doesn't cause accept edit
                return;
            }

            var isRootCanvas = _ownerItem == null;
            if (isRootCanvas)
                //no accept routines for root canvas
                return;

            throw new NotImplementedException("Accept item");
        }
    }
}
