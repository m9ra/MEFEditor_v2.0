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

namespace MEFEditor.TestConsole.Drawings
{
    /// <summary>
    /// Interaction logic for TestForm.xaml. It is a GUI where
    /// editor can be loaded outside of Visual Studio.
    /// </summary>
    public partial class TestForm : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestForm"/> class.
        /// </summary>
        public TestForm()
        {
            InitializeComponent();

            Closed += (sender, arg) => Environment.Exit(0);
            KeyDown += (sender, arg) =>
            {
                if (arg.Key == Key.Escape) Environment.Exit(0);
            };
        }
    }
}
