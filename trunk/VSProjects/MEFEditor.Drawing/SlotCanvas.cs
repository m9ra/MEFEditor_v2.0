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
    /// <summary>
    /// Canvas that can be used as slot filled by <see cref="DiagramItem"/>.
    /// Drawing of slot is defined by <see cref="SlotDefinition"/>.
    /// </summary>
    public class SlotCanvas:DiagramCanvasBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SlotCanvas"/> class.
        /// </summary>
        public SlotCanvas()
        {
            MinHeight = 100;
            MinWidth = 100;
            Background = Brushes.Transparent;
        }
    }
}
