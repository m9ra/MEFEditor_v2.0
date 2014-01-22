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

namespace MEFEditor
{
    /// <summary>
    /// Interaction logic for EditorGUI.xaml
    /// </summary>
    public partial class EditorGUI : UserControl
    {
        public DiagramCanvas Workspace { get { return _Workspace; } }

        public StackPanel Log { get { return _Log; } }

        public ComboBox CompositionPoints { get { return _CompositionPoints; } }

        public EditorGUI()
        {
            InitializeComponent();
        }
    }
}
