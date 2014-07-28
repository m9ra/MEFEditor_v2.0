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
    /// Drawing definition for <see cref="DirectoryCatalog" />.
    /// </summary>
    public partial class DirectoryCatalogDrawing : ContentDrawing
    {
        /// <summary>
        /// Cached image for icon.
        /// </summary>
        protected static readonly CachedImage Image = new CachedImage(Icons.Folder);

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryCatalogDrawing" /> class.
        /// </summary>
        /// <param name="item">The item.</param>
        public DirectoryCatalogDrawing(DiagramItem item)
            :base(item)
        {
            InitializeComponent();

            DrawingTools.SetToolTip(Caption, Definition.DrawedType);
            DrawingTools.SetImage(CaptionIcon, Image);
            InstanceID.Text = Definition.ID;

            var path = Definition.GetPropertyValue("Path");
            var fullPath = Definition.GetPropertyValue("FullPath");
            var pattern = Definition.GetPropertyValue("Pattern");
            var error = Definition.GetPropertyValue("Error");

            Path.Text = path;
            Pattern.Text = pattern;

            var fullPathInfo = DrawingTools.GetHeadingText("FullPath", fullPath);
            DrawingTools.SetToolTip(PathDock, fullPath);

            //Display error message if needed
            ErrorText.Text = error;
            var hasError = error != null;
            ErrorDock.Visibility = hasError ? Visibility.Visible : Visibility.Hidden;

            Item.FillSlot(Components, Definition.Slots.First());
        }
    }
}
