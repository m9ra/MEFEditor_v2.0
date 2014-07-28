using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.ComponentModel.Composition.Hosting;

using MEFEditor.Drawing;

namespace RecommendedExtensions.Core.Drawings
{
    /// <summary>
    /// Drawing definition for <see cref="CompositionBatch" />.
    /// </summary>
    public partial class CompositionBatchDrawing : ContentDrawing
    {
        /// <summary>
        /// Cached image for icon.
        /// </summary>
        protected static readonly CachedImage Image = new CachedImage(Icons.Batch);

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositionBatchDrawing"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        public CompositionBatchDrawing(DiagramItem item)
            : base(item)
        {
            InitializeComponent();

            DrawingTools.SetToolTip(Caption, Definition.DrawedType);
            DrawingTools.SetImage(CaptionIcon, Image);
            InstanceID.Text = Definition.ID;

            var slot = Definition.Slots.First();
            Item.FillSlot(Components, slot);
        }
    }
}
