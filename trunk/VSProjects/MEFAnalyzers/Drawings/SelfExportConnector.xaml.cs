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
    public partial class SelfExportConnector : ConnectorDrawing
    {
        private static readonly Dictionary<string, string> ExportProperties = new Dictionary<string, string>()
        {
             {"Contract","Contract"}, 
             {"ContractType","Contract type"},
             {"ItemType","Item type"}
        };

        public SelfExportConnector(ConnectorDefinition definition, DiagramItem owningItem)
            : base(definition, ConnectorAlign.Top, owningItem)
        {
            InitializeComponent();

            Contract.Text = definition.GetProperty("Contract").Value;
            ConnectorTools.SetProperties(this, "Self Export info", ExportProperties);
            ConnectorTools.SetMessages(ErrorOutput, definition);
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
