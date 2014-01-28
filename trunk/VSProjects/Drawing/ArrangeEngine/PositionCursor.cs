using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

namespace Drawing.ArrangeEngine
{
    class PositionCursor
    {
        static readonly double Margin = 50;

        double _lineOffset = Margin;

        internal void RegisterPosition(Point oldPosition, Size size)
        {
            _lineOffset = Math.Max(_lineOffset, oldPosition.X + size.Width);
        }

        internal Point CreateNextPosition(Size size)
        {
            var result = new Point(_lineOffset, 50);

            _lineOffset += size.Width;
            return result;
        }
    }
}
