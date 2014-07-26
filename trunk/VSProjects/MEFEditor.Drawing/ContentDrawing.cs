using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

using System.Windows.Controls;

namespace MEFEditor.Drawing
{
    /// <summary>
    /// Represents content drawing displayed inside diagram item
    /// </summary>
    public abstract class ContentDrawing : Border
    {
        public readonly DiagramItem Item;

        public DiagramItemDefinition Definition { get { return Item.Definition; } }

        public ContentDrawing(DiagramItem item)
        {
            Item = item;
        }
    }
}
