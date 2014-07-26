using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

namespace MEFEditor.Drawing.Behaviours
{
    abstract class DropStrategyBase
    {
        internal Exception DropException;

        protected DiagramCanvasBase DropTarget { get; private set; }
        protected DragAdorner DragAdorner { get; private set; }
        protected DiagramItem DragItem { get { return DragAdorner.Item; } }
        protected DiagramItem OwnerItem { get { return DropTarget.OwnerItem; } }
        protected DiagramContext Context { get { return DropTarget.DiagramContext; } }
        protected DiagramDefinition Diagram { get { return Context.Diagram; } }
        protected DragEventArgs E { get; private set; }
        protected string Hint { get { return DragAdorner.Hint; } set { DragAdorner.Hint = value; } }
        protected EditViewBase CurrentView;


        protected abstract void onDrop();
        protected abstract void onDropEnd();

        protected abstract void move();

        protected abstract bool exclude();
        protected abstract bool rootExclude();

        protected abstract void rootAccept();
        protected abstract void accept();

        internal void OnDrop(DiagramCanvasBase dropTarget, DragEventArgs e)
        {
            DropException = null;

            onDrop(dropTarget, e);

   /*         try
            {
                onDrop(dropTarget, e);
            }
            catch (Exception ex)
            {
                DropException = ex;
            }*/
        }

        private void onDrop(DiagramCanvasBase dropTarget, DragEventArgs e)
        {
            DragAdorner = e.Data.GetData("DragAdorner") as DragAdorner;
            if (DragAdorner == null)
                return;
            E = e;
            DropTarget = dropTarget;

            E.Handled = true;

            CurrentView = Diagram.InitialView;
            onDrop();

            if (DropTarget.OwnerItem == DragItem)
            {
                //cant drop self to sub slot
                move();
                onDropEnd();
                return;
            }

            if (DragItem.ContainingDiagramCanvas == DropTarget)
            {
                //move within dropTarget canvas
                move();
                onDropEnd();
                return;
            }

            if (excludeFromParent())
            {
                acceptItem();
            }

            onDropEnd();
        }

        private bool excludeFromParent()
        {
            if (DragItem.IsRootItem)
            {
                return rootExclude();
            }
            else
            {
                return exclude();
            }
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
