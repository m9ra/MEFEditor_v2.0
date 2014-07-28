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
    /// Represents content drawing displayed inside diagram item.
    /// </summary>
    public abstract class ContentDrawing : Border
    {
        /// <summary>
        /// The item where content is displayed.
        /// </summary>
        public readonly DiagramItem Item;

        /// <summary>
        /// Gets the item definition.
        /// </summary>
        /// <value>The definition.</value>
        public DiagramItemDefinition Definition { get { return Item.Definition; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentDrawing" /> class.
        /// </summary>
        /// <param name="item">The item where content is displayed.</param>
        public ContentDrawing(DiagramItem item)
        {
            Item = item;
        }
    }
}
