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

using Analyzing;
using TypeSystem;

using MEFAnalyzers.Drawings;

namespace MEFAnalyzers.Dialogs
{
    /// <summary>
    /// Interaction logic for ComponentTypeDialog.xaml
    /// </summary>
    public partial class ComponentType : Window
    {
        public InstanceInfo SelectedComponent { get; private set; }

        public ComponentType(IEnumerable<ComponentInfo> components)
        {
            InitializeComponent();

            foreach (var component in components)
            {
                //TODO add assembly info

                var componentType = component.ComponentType;

                var item = new ListBoxItem();
                var text = DrawingTools.GetText(componentType.TypeName);
                item.Content = text;
                item.BorderThickness = new Thickness(1);
                item.Padding = new Thickness(10);
                item.BorderBrush = Brushes.Gray;
                item.FontSize = 15;

                var assemblyText=DrawingTools.GetHeadingText("Assembly", component.DefiningAssembly.ToString());
                DrawingTools.SetToolTip(item, assemblyText);

                item.Selected += (s, e) =>
                {
                    SelectedComponent = componentType;
                    DialogResult = true;
                };

                ComponentTypes.Items.Add(item);
            }
        }
    }
}
