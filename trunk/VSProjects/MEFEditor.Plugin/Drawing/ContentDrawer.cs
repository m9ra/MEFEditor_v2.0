using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

using MEFEditor.Drawing;

namespace MEFEditor.Plugin.Drawing
{
    /// <summary>
    /// Delegate for providing drawing of items.
    /// </summary>
    /// <param name="item">The item which drawing will be provided.</param>
    /// <returns>Created drawing.</returns>
    public delegate ContentDrawing ContentProvider(DiagramItem item);

    /// <summary>
    /// Representation of item drawing provider, according to 
    /// item type.
    /// </summary>
    public class ContentDrawer
    {
        /// <summary>
        /// Type which content will be provided by content provider.
        /// </summary>
        public readonly string DrawedType;

        /// <summary>
        /// Determine that this drawer is used when no matching drawer for drawed type is found.
        /// </summary>
        /// <value><c>true</c> if this instance is default drawer; otherwise, <c>false</c>.</value>
        public bool IsDefaultDrawer { get { return DrawedType == null || DrawedType == ""; } }

        /// <summary>
        /// The drawing provider of items.
        /// </summary>
        public readonly ContentProvider Provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentDrawer"/> class.
        /// </summary>
        /// <param name="drawedType">Type which content will be provided by current drawer.</param>
        /// <param name="provider">The provider of items' drawings.</param>
        public ContentDrawer(string drawedType, ContentProvider provider)
        {
            DrawedType = drawedType;
            Provider = provider;
        }
    }
}
