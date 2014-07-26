using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace MEFEditor.Drawing
{
    public class SlotCanvas:DiagramCanvasBase
    {
        public SlotCanvas()
        {
            MinHeight = 100;
            MinWidth = 100;
            Background = Brushes.Transparent;
        }
    }
}
