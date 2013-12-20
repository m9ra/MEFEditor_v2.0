using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Documents;


namespace Drawing.Behaviours
{
    internal delegate Point GetPosition(FrameworkElement element);

    internal delegate void SetPosition(FrameworkElement element, Point position);

    internal class DragAndDrop
    {
        private readonly DiagramItem _item;
        private readonly GetPosition _get;
        private readonly SetPosition _set;

        internal static void Attach(DiagramItem attachedElement, GetPosition get, SetPosition set)
        {
            new DragAndDrop(attachedElement, get, set);
        }

        private DragAndDrop(DiagramItem attachedElement, GetPosition get, SetPosition set)
        {
            _item = attachedElement;
            _get = get;
            _set = set;

            hookEvents();
        }

        private void hookEvents()
        {
            _item.MouseDown += onMouseDown;
            _item.DragLeave += _item_DragOver;
        }

        void _item_DragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void onMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            e.Handled = true;

            _item.CaptureMouse();

            var adorner = new DragAdorner(_item, e.GetPosition(_item.Output));
            adorner.DragStart();

            var data = new DataObject("DragAdorner", adorner);
            _item.DiagramContext.Diagram.DragStart(_item);
            try
            {
                var effect = DragDrop.DoDragDrop(_item, data, DragDropEffects.Move);

                var view = adorner.EditView;
                if (view != null && effect != DragDropEffects.None)
                {
                    view.Commit();
                }
            }
            finally
            {
                _item.DiagramContext.Diagram.DragEnd();
            }

            if (DiagramCanvasBase.DropStrategy.DropException != null)
            {
                throw new Utilities.ExceptionWrapper(DiagramCanvasBase.DropStrategy.DropException);
            }

            adorner.DragEnd();
            _item.ReleaseMouseCapture();

            /*  var diff = adorner.GlobalPosition - _item.GlobalPosition;

              var currPos = _get(_item);
              currPos.X += diff.X;
              currPos.Y += diff.Y;

              _set(_item, currPos);  */
        }
    }
}
