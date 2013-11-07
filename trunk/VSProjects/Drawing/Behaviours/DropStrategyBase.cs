using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

namespace Drawing.Behaviours
{
    abstract class DropStrategyBase
    {
        protected DiagramCanvasBase DropTarget { get; private set; }
        protected DragAdorner DragAdorner { get; private set; }
        protected DiagramItem DragItem { get { return DragAdorner.Item; } }
        protected DiagramItem OwnerItem { get { return DropTarget.OwnerItem; } }
        protected DiagramContext Context { get { return DropTarget.DiagramContext; } }
        protected DragEventArgs E { get; private set; }
        protected string Hint { get { return DragAdorner.Hint; } set { DragAdorner.Hint = value; } }

        protected abstract void onDrop();


        protected abstract void move();

        protected abstract void exclude();
        protected abstract void rootExclude();

        protected abstract void rootAccept();
        protected abstract void accept();

        internal void OnDrop(DiagramCanvasBase dropTarget, DragEventArgs e)
        {
            DragAdorner = e.Data.GetData("DragAdorner") as DragAdorner;
            if (DragAdorner == null)
                return;
            E = e;
            DropTarget = dropTarget;

            E.Handled = true;

            onDrop();

            if (DropTarget.OwnerItem == DragItem)
            {
                //cant drop self to sub slot
                move();
                return;
            }

            if (DragItem.ContainingDiagramCanvas == DropTarget)
            {
                //move within dropTarget canvas
                move();
                return;
            }

            excludeFromParent();
            acceptItem();
        }

        private void excludeFromParent()
        {
            if (DragItem.IsRootItem)
            {
                //no drop action
                rootExclude();
                return;
            }
            exclude();
        }

        private void acceptItem()
        {
            var dragItem = DragAdorner.Item;

            if (dragItem.ContainingDiagramCanvas == DropTarget)
            {
                //item moving doesn't cause accept edit
                return;
            }

            var isRootCanvas = OwnerItem == null;
            if (isRootCanvas)
            {
                //no accept routines for root canvas
                rootAccept();
                return;
            }

            accept();
        }
    }
}
