using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Windows;
using System.Windows.Media;

using System.ComponentModel;


namespace Drawing
{
    public class DiagramCanvas : DiagramCanvasBase
    {
        private readonly ScaleTransform _scale = new ScaleTransform();

        private Vector _shift;


        public Vector Shift
        {
            get
            {
                return _shift;
            }

            set
            {
                if (_shift == value)
                    return;

                _shift = value;
                InvalidateArrange();
            }
        }

        public double Zoom
        {
            get
            {
                //both scale factors has same value
                return _scale.ScaleX;
            }
            set
            {
                if (_scale.ScaleX == value)
                    return;

                _scale.ScaleX = value;
                _scale.ScaleY = value;

                InvalidateArrange();
            }
        }


        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (DiagramContext != null)
            {
                DiagramContext.Provider.Engine.ArrangeChildren(OwnerItem, this);
            }

            foreach (FrameworkElement child in Children)
            {
                var position = GetPosition(child);

                child.RenderTransform = _scale;

                var scaledPosition = _scale.Transform(position);
                var shiftedPosition = new Point(scaledPosition.X + Shift.X, scaledPosition.Y + Shift.Y);

                child.Arrange(new Rect(shiftedPosition, child.DesiredSize));
            }
            return arrangeSize;
        }
    }
}
