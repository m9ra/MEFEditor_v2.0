using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Windows;
using System.ComponentModel;


namespace Drawing
{
    public class DiagramCanvas : DiagramCanvasBase
    {
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



        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (DiagramContext != null)
            {
                DiagramContext.Provider.Engine.ArrangeChildren(OwnerItem, this);
            }

            foreach (FrameworkElement child in Children)
            {
                var position = GetPosition(child);

                var shiftedPosition = new Point(position.X + Shift.X, position.Y + Shift.Y);
                child.Arrange(new Rect(shiftedPosition, child.DesiredSize));
            }
            return arrangeSize;
        }
    }
}
