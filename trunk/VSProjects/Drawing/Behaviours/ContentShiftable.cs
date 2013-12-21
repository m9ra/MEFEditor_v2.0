using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Input;

namespace Drawing.Behaviours
{
    class ContentShiftable
    {
        private readonly DiagramCanvas _canvas;

        private Point _lastPos;

        private bool _isShifting;


        internal static void Attach(DiagramCanvas canvas)
        {
            new ContentShiftable(canvas);
        }

        private ContentShiftable(DiagramCanvas canvas)
        {
            _canvas = canvas;

            _canvas.MouseDown += canvas_MouseDown;
            _canvas.MouseUp += _canvas_MouseUp;

        }

        void _canvas_MouseMove(object sender, MouseEventArgs e)
        {
            var currPos = e.GetPosition(_canvas);

            var shiftDelta = currPos - _lastPos;
            _lastPos = currPos;

            _canvas.Shift += shiftDelta;
        }

        void _canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isShifting)
                return;

            Mouse.OverrideCursor = null;

            _canvas.MouseMove -= _canvas_MouseMove;
            _isShifting = false;
            _canvas.ReleaseMouseCapture();
        }

        void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            if (_isShifting)
                return;

            _canvas.CaptureMouse();

            Mouse.OverrideCursor = Cursors.SizeAll;

            _isShifting = true;
            _lastPos = e.GetPosition(_canvas);

            _canvas.MouseMove += _canvas_MouseMove;
        }
    }
}
