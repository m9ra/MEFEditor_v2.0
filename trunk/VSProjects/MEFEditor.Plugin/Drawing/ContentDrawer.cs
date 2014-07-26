using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

using MEFEditor.Drawing;

namespace MEFEditor.Plugin.Drawing
{
    public delegate ContentDrawing ContentProvider(DiagramItem item);

    public class ContentDrawer
    {
        /// <summary>
        /// Type which content will be provided by content provider
        /// </summary>
        public readonly string DrawedType;

        /// <summary>
        /// Determine that this drawer is used when no matching drawer for drawed type is found
        /// </summary>
        public bool IsDefaultDrawer { get { return DrawedType == null || DrawedType == ""; } }

        public readonly ContentProvider Provider;

        public ContentDrawer(string drawedType, ContentProvider provider)
        {
            DrawedType = drawedType;
            Provider = provider;
        }
    }
}
