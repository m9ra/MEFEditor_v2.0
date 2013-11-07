using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

namespace Drawing.Behaviours
{
    class PreviewDropStrategy : DropStrategyBase
    {
        protected override void move()
        {
            DragAdorner.Hint = "Change item position";
            E.Effects = DragDropEffects.Move;
        }

        protected override void onDrop()
        {
            Hint = "";
        }

        protected override void exclude()
        {
            if (DragItem.CanExcludeFromParent)
            {
                Hint = string.Format("Exclude from: '{0}'", DragItem.ParentItem.ID);

                if (OwnerItem != null)
                {
                    throw new NotImplementedException("Determine that parent can accept");
                    //hint += string.Format("\nAccept to '{0}'", _ownerItem.ID);
                }
            }
            else
            {
                E.Effects = DragDropEffects.None;
                Hint = string.Format("Can't exclude from: '{0}'", DragItem.ParentItem.ID);
            }
        }

        protected override void rootExclude()
        {
            //nothing to do
        }

        protected override void rootAccept()
        {
            //notihng to do
        }

        protected override void accept()
        {
            var acceptTarget = DropTarget.OwnerItem.ID;
            foreach (var accept in DropTarget.OwnerItem.AcceptEdits)
            {
                if (accept.Action(true))
                {
                    Hint += string.Format("Accept by '{0}'", acceptTarget);
                    return;
                }
            }

            E.Effects = DragDropEffects.None;
            Hint = string.Format("Cannot accept by '{0}'", acceptTarget);
        }


    }
}
