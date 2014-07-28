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

using MEFEditor.Drawing;

namespace RecommendedExtensions.Core.Drawings
{
    /// <summary>
    /// Drawing definition for <see cref="AggregateCatalog" />.
    /// </summary>
    public partial class AggregateCatalogDrawing : ContentDrawing
    {
        /// <summary>
        /// Cached image for icon.
        /// </summary>
        protected static readonly CachedImage Image = new CachedImage(Icons.Container);

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateCatalogDrawing" /> class.
        /// </summary>
        /// <param name="item">The item.</param>
        public AggregateCatalogDrawing(DiagramItem item)
            : base(item)
        {

            InitializeComponent();

            DrawingTools.SetToolTip(Caption, Definition.DrawedType);
            DrawingTools.SetImage(CaptionIcon, Image);
            InstanceID.Text = Definition.ID;

            var slot = Definition.Slots.First();
            Item.FillSlot(Catalogs, slot);
        }
    }
}
