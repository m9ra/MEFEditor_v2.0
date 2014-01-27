using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

namespace Drawing.Behaviours
{
    class DropStrategy : DropStrategyBase
    {
        protected override void onDrop()
        {
            if (E.Effects == DragDropEffects.None)
                return;

            DragItem.GlobalPosition = DragAdorner.GlobalPosition;
        }

        protected override bool exclude()
        {
            if (DragItem.CanExcludeFromParent)
            {
                hintParentPosition();

                CurrentView = DragItem.ParentExcludeEdit.Action(CurrentView);
            }
            else
            {
                throw new NotSupportedException("Cannot exclude");
            }
            return true;
        }

        protected override bool rootExclude()
        {
            //nothing to do
            hintParentPosition();
            return true;
        }

        protected override void rootAccept()
        {
            //nothing to do
        }

        protected override void accept()
        {
            foreach (var accept in DropTarget.OwnerItem.AcceptEdits)
            {
                var accepted = accept.Action(CurrentView);
                if (!accepted.HasError)
                {
                    CurrentView = accepted;
                    return;
                }
            }

            throw new NotSupportedException("Cannot accept");
        }

        protected override void move()
        {
            //nothing to do
        }

        private void hintParentPosition()
        {
            var diff = DragItem.GlobalPosition - DropTarget.GlobalPosition;
            Context.HintPosition(OwnerItem, DragItem, new Point(diff.X, diff.Y));
        }

        protected override void onDropEnd()
        {
            if (CurrentView != Diagram.InitialView)
                DragAdorner.EditView = CurrentView;

            //else there is nothing to commit
        }
    }
}
