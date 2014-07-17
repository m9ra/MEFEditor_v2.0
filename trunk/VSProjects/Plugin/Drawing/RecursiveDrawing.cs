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

namespace MEFEditor.Plugin.Drawing
{
    public class RecursiveDrawing : ContentDrawing
    {
        public RecursiveDrawing(DiagramItem item)
            : base(item)
        {
            BorderThickness = new Thickness(4);
            BorderBrush = Brushes.Red;
            Background = Brushes.LightGray;
            
            var warningText = new TextBlock();
            warningText.Text = "!Circular dependency detected!";
            warningText.Foreground = Brushes.Red;
            warningText.FontSize = 20;            

            var description = new TextBlock();
            description.Text = "Instance '" + Definition.ID + "' should be displayed here.";

            var layout = new StackPanel();
            layout.Margin = new Thickness(20);
            layout.Children.Add(warningText);
            layout.Children.Add(description);

            Child = layout;
        }
    }
}
