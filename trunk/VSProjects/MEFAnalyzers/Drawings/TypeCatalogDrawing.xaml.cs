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
    public partial class TypeCatalogDrawing : ContentDrawing
    {
        protected static readonly CachedImage Image = new CachedImage(Icons.Type);

        public TypeCatalogDrawing(DiagramItem item)
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
