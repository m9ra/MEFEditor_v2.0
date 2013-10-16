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

namespace Research.Drawings
{
    /// <summary>
    /// Interaction logic for TestForm.xaml
    /// </summary>
    public partial class TestForm : Window
    {
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
