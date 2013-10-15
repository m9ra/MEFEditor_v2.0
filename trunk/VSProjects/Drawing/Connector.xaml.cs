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

namespace Drawing
{
    /// <summary>
    /// Interaction logic for Connector.xaml
    /// </summary>
    public partial class Connector : UserControl
    {
        public readonly JoinPointDefinition Definition;
        /// <summary>
        /// Point where line will be connected to (relative to connector's position)
        /// </summary>
        public Point ConnectPoint { get; private set; }

        public Connector(JoinPointDefinition definition)
        {
            Definition = definition;    
            InitializeComponent();
        }
    }
}
