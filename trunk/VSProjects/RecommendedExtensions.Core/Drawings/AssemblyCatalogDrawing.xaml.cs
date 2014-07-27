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
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class AssemblyCatalogDrawing : ContentDrawing
    {
        protected static readonly CachedImage Image = new CachedImage(Icons.Assembly);

        public AssemblyCatalogDrawing(DiagramItem item)
            : base(item)
        {
            InitializeComponent();

            DrawingTools.SetToolTip(Caption, Definition.DrawedType);
            DrawingTools.SetImage(CaptionIcon, Image);
            InstanceID.Text = Definition.ID;

            var path = Definition.GetPropertyValue("Path");
            var fullPath = Definition.GetPropertyValue("FullPath");
            var error = Definition.GetPropertyValue("Error");
            var assemblyName = Definition.GetPropertyValue("AssemblyName");

            Path.Text = path;
            if (assemblyName == null)
            {
                AssemblyNameDock.Visibility = Visibility.Hidden;
            }
            else
            {
                AssemblyName.Text = assemblyName;
            }

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
