using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

using System.Windows.Controls;

namespace Drawing
{
    /// <summary>
    /// Represents content drawing displayed inside diagram item
    /// </summary>
    public abstract class ContentDrawing:Border
    {
        public readonly DrawingDefinition Definition;

        public ContentDrawing(DrawingDefinition definition)
        {
            Definition = definition;
        }
    }
}
