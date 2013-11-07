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
            DragItem.GlobalPosition = DragAdorner.GlobalPosition;
        }

        protected override void exclude()
        {
            if (DragItem.CanExcludeFromParent)
            {
                var diff = DragItem.GlobalPosition - DropTarget.GlobalPosition;
                Context.HintPosition(OwnerItem, DragItem, new Point(diff.X, diff.Y));
                if (!DragItem.ParentExcludeEdit.Action(false))
                    throw new NotImplementedException();
            }
            else
            {
                throw new NotSupportedException("Cannot exclude");
            }
        }

        protected override void rootExclude()
        {
            //nothing to do
        }

        protected override void rootAccept()
        {
            //nothing to do
        }

        protected override void accept()
        {
            foreach (var accept in DragAdorner.Item.AcceptEdits)
            {
                if (accept.Action(false))
                    return;
            }
        }

        protected override void move()
        {
            //nothing to do
        }
    }
}
