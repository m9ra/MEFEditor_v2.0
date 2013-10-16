using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace Drawing.Behaviours
{
    internal delegate Point GetPosition(FrameworkElement element);

    internal delegate void SetPosition(FrameworkElement element, Point position);

    internal class DragAndDrop
    {
        private readonly FrameworkElement _element;
        private readonly GetPosition _get;
        private readonly SetPosition _set;

        private Point _lastDragPosition;
        private bool _isDragStarted;

        internal static void Attach(FrameworkElement attachedElement, GetPosition get, SetPosition set)
        {
            new DragAndDrop(attachedElement,get,set);
        }

        private DragAndDrop(FrameworkElement attachedElement,GetPosition get, SetPosition set)
        {
            _element = attachedElement;
            _get = get;
            _set = set;

            hookEvents();
        }

        private void hookEvents()
        {
            _element.MouseMove += onMouseMove;
            _element.MouseDown += onMouseDown;
            _element.MouseUp += onMouseUp;
        }

        private void onMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragStarted)
                return;

            var currentMousePos = e.GetPosition(null);
            var shift = currentMousePos - _lastDragPosition;
            _lastDragPosition = currentMousePos;

            var currentItemPos = _get(_element);

            currentItemPos += shift;
            _set(_element, currentItemPos);
        }

        private void onMouseDown(object sender, MouseButtonEventArgs e)
        {
            _element.CaptureMouse();

            _lastDragPosition = e.GetPosition(null);
            _isDragStarted = true;
        }

        private void onMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDragStarted = false;
            _element.ReleaseMouseCapture();
        }

    }
}
