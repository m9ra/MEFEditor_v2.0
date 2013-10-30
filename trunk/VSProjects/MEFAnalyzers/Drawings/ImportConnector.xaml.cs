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
    public partial class ImportConnector : ConnectorDrawing
    {
        private static readonly Dictionary<string, string> ImportProperties = new Dictionary<string, string>()
        {
             {"ContractType","Contract type"},
             {"AllowMany","Allow importing many exports"},
             {"AllowDefault","Allow default value of import"},
             {"IsPrerequisity","Is import prerequisity"}
        };

        public ImportConnector(ConnectorDefinition definition, DiagramItem owningItem)
            : base(definition, ConnectorAlign.Left, owningItem)
        {
            InitializeComponent();
            Contract.Text = definition.GetProperty("Contract").Value;

            var tooltip = DrawingTools.ConnectorProperties(definition, "Import info", ImportProperties);
            DrawingTools.SetToolTip(tooltip, this);
        }

        public override Point ConnectPoint
        {
            get
            {
                var res = new Point(-5, -5);
                res = this.TranslatePoint(res, Glyph);

                res = new Point(-res.X, -res.Y);
                return res;
            }
        }
    }
}
