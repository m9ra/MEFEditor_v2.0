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
    public partial class AggregateCatalogDrawing : ContentDrawing
    {
        public AggregateCatalogDrawing(DiagramItem item)
            : base(item)
        {

            InitializeComponent();

            DrawingTools.SetToolTip(Caption, Definition.DrawedType);
            DrawingTools.SetIcon(CaptionIcon, Icons.Container);
            InstanceID.Text = Definition.ID;

            foreach (var property in Definition.Properties)
            {
                var propertyBlock = new TextBlock();
                propertyBlock.Text = string.Format("{0}: {1}", property.Name, property.Value);

                Properties.Children.Add(propertyBlock);
            }

            var slot = Definition.Slots.First();
            Item.FillSlot(Catalogs, slot);
        }
    }
}
