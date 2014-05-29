using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

namespace Drawing.Behaviours
{
    class MouseHint : Adorner
    {
        private readonly static Typeface ArialFace = new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);

        private readonly static Pen BorderPen = new Pen(Brushes.Gray, 1);

        private readonly static Brush BackgroundBrush = new LinearGradientBrush(Colors.White, Colors.LightGray, 90);

        private readonly UIElement _mouseScope;

        private readonly AdornerLayer _layer;

        private Point _cursorPosition;

        internal double Padding { get; set; }

        internal double CursorMargin { get; set; }

        internal string HintText { get; set; }



        internal MouseHint(UIElement mouseScope)
            : base(mouseScope)
        {
            _mouseScope = mouseScope;

            _layer = AdornerLayer.GetAdornerLayer(_mouseScope);
            _layer.Add(this);

            HintText = "";
            Padding = 5;
            CursorMargin = 5;
        }

        internal void UpdateCursor(Point position)
        {
            _cursorPosition = position;
            _layer.Update(_mouseScope);
        }

        internal void HintEnd()
        {
            _layer.Remove(this);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (HintText == null)
                return;

            var formattedHint = new FormattedText(HintText, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, ArialFace, 12, Brushes.Blue);

            var pos = new Point(_cursorPosition.X + CursorMargin, _cursorPosition.Y + CursorMargin);
            var textPos = new Point(pos.X + 3 * Padding, pos.Y + 3 * Padding);
            var fontRectPos = new Point(textPos.X - Padding, textPos.Y - Padding);

            var fontRect = new Rect(fontRectPos, new Size(formattedHint.Width + Padding * 2, formattedHint.Height + Padding * 2));


            drawingContext.DrawRoundedRectangle(BackgroundBrush, BorderPen, fontRect, 5, 5);
            drawingContext.DrawText(formattedHint, textPos);
        }
    }
}
