using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

using Drawing;

namespace Research.Drawings
{
    public delegate ContentDrawing ContentProvider(DrawingDefinition definition);

    public class ContentDrawer
    {
        /// <summary>
        /// Type which content will be provided by content provider
        /// </summary>
        public readonly string DrawedType;

        /// <summary>
        /// Determine that this drawer is used when no matching drawer for drawed type is found
        /// </summary>
        public bool IsDefaultDrawer { get { return DrawedType == null; } }

        public readonly ContentProvider Provider;

        public ContentDrawer(string drawedType, ContentProvider provider)
        {
            DrawedType = drawedType;
            Provider = provider;
        }
    }
}
