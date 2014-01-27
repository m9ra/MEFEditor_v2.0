using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

using TypeSystem;

namespace Research.GUI
{
    class AssemblyItem : Border
    {
        public AssemblyItem(AssemblyProvider assembly)
        {
            var body = new StackPanel();

            var assemblyName = new TextBlock();
            assemblyName.Text = assembly.Name;
            assemblyName.FontWeight = FontWeights.Bold;

            var assemblyPath = new TextBlock();
            assemblyPath.Text = "FullPath: " + assembly.FullPath;
            assemblyPath.FontWeight = FontWeights.Light;

            var mappingPath = new TextBlock();
            mappingPath.Text = "Mapping: " + assembly.FullPath;
            mappingPath.FontWeight = FontWeights.Light;

            body.Children.Add(assemblyName);
            body.Children.Add(assemblyPath);
            body.Children.Add(mappingPath);

            Child = body;

            Padding = new Thickness(5);
            Margin = new Thickness(0, 1, 0, 1);
            Background = GUIColors.SectionForeground;
        }
    }
}
