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
    public partial class ComponentDrawing : ContentDrawing
    {
        public ComponentDrawing(DiagramItem item)
            : base(item)
        {
            InitializeComponent();

            DrawingTools.SetIcon(ComponentIcon, Icons.Component);

            TypeName.Text = Definition.DrawedType;
            InstanceID.Text = Definition.ID;

            var properties = Definition.Properties.OrderBy((prop) => prop.Name);

            string removedBy = null;
            var isEntryInstance = false;

            foreach (var property in properties)
            {
                var propertyBlock = new TextBlock();

                var value = property.Value;
                var name = property.Name;
                var prefix = value == null || value == "" ? name : name + ": ";

                propertyBlock.Text = prefix + value;

                Properties.Children.Add(propertyBlock);

                isEntryInstance |= property.Name == "EntryInstance";
                if (property.Name == "Removed")
                {
                    removedBy = property.Value;
                }
            }

            if (isEntryInstance)
            {
                BorderBrush = Brushes.DarkGreen;
                BorderThickness = new Thickness(6);
            }

            var isRemoved = removedBy != null;
            if (isRemoved)
            {
                DrawingTools.SetIcon(RemoveIcon, Icons.Remove);
                DrawingTools.SetToolTip(RemoveIcon, DrawingTools.GetText("Component has been removed " + removedBy));
            }
            RemoveIcon.Visibility = isRemoved ? Visibility.Visible : Visibility.Hidden;

        }
    }
}
