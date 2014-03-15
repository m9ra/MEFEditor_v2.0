using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

using TypeSystem;

namespace Plugin.GUI
{
    /// <summary>
    /// Visual item representing AssemblyProvider. Provide ability for setting assembly configuration properties.
    /// </summary>
    class AssemblyItem : Border
    {
        /// <summary>
        /// Textblock where mapping is displayed
        /// </summary>
        private readonly TextBlock _mappingPath = new TextBlock();

        /// <summary>
        /// Assembly represented by current item
        /// </summary>
        private readonly AssemblyProvider _assembly;

        /// <summary>
        /// Event fired whenever mapping of assembly is changed
        /// </summary>
        public event AssemblyEvent MappingChanged;

        public AssemblyItem(AssemblyProvider assembly)
        {
            _assembly = assembly;

            //create item body
            var body = new StackPanel();
            Child = body;

            //create name block
            var assemblyName = new TextBlock();
            assemblyName.Text = assembly.Name;
            assemblyName.FontWeight = FontWeights.Bold;

            //create path block
            var assemblyPath = new TextBlock();
            assemblyPath.Text = "FullPath: " + assembly.FullPath;
            assemblyPath.FontWeight = FontWeights.Light;

            //initialize mapping block
            onMappingChanged();
            _mappingPath.FontWeight = FontWeights.Light;
            _assembly.MappingChanged += (a) => onMappingChanged();

            //arrange layout
            body.Children.Add(assemblyName);
            body.Children.Add(assemblyPath);
            body.Children.Add(_mappingPath);

            //create menu for mapping settings
            var mappingSetter = new MenuItem();
            mappingSetter.Header = "Set mapping";
            mappingSetter.Click += mappingSetter_Click;

            ContextMenu = new ContextMenu();
            ContextMenu.Items.Add(mappingSetter);

            //tune item appearance
            Padding = new Thickness(5);
            Margin = new Thickness(0, 1, 0, 1);
            Background = GUIColors.SectionForeground;
        }

        #region Event handlers

        private void mappingSetter_Click(object sender, RoutedEventArgs e)
        {
            var newMapping = Microsoft.VisualBasic.Interaction.InputBox(@"Please write down full path for mapping in form `C:\MyFolder\MyAssembly.exe`. This mapping will be used to override real path of assembly.", "Set mappping path", _assembly.FullPathMapping);

            if (newMapping == null || newMapping == "")
                //value has been discarded
                return;

            _assembly.FullPathMapping = newMapping;
        }

        private void onMappingChanged()
        {
            _mappingPath.Text = "Mapping: " + _assembly.FullPathMapping;

            if (MappingChanged != null)
                MappingChanged(_assembly);
        }

        #endregion
    }
}
