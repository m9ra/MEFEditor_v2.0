using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Input;

namespace Drawing.Behaviours
{
    class ContentZoomable
    {
        private readonly DiagramCanvas _canvas;

        internal static void Attach(DiagramCanvas canvas)
        {
            new ContentZoomable(canvas);
        }

        private ContentZoomable(DiagramCanvas canvas)
        {
            _canvas = canvas;

            _canvas.MouseWheel += _canvas_MouseWheel;
        }

        void _canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var step = e.Delta;

            var scaleFactor = 0.1;

            if (step > 0)
            {
                _canvas.Zoom *= 1 + scaleFactor;
            }
            else
            {
                _canvas.Zoom /= 1 + scaleFactor;
            }
        }
    }
}
