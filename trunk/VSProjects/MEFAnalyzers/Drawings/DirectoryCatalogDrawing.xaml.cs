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

using Drawing;

namespace MEFAnalyzers.Drawings
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class DirectoryCatalogDrawing : ContentDrawing
    {
        public DirectoryCatalogDrawing(DiagramItem item)
            :base(item)
        {
            InitializeComponent();

            DrawingTools.SetToolTip(CaptionText, Definition.DrawedType);
            DrawingTools.SetIcon(CaptionIcon, Icons.Folder);

            var path = Definition.GetProperty("Path");
            var pattern = Definition.GetProperty("Pattern");

            Path.Text = path.Value;
            Pattern.Text = pattern.Value;
       
            Item.FillSlot(Components, Definition.Slots.First());
        }
    }
}
