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

using MEFEditor.Drawing;

namespace RecommendedExtensions.Core.Drawings
{
    /// <summary>
    /// Drawing definition for import connector.
    /// </summary>
    public partial class ImportConnector : ConnectorDrawing
    {
        /// <summary>
        /// The import properties.
        /// </summary>
        private static readonly Dictionary<string, string> ImportProperties = new Dictionary<string, string>()
        {
             {"Contract","Contract"},
             {"ContractType","Contract type"},
             {"ContractItemType","Item type"},
             {"AllowMany","Allow many"},
             {"AllowDefault","Allow default"},
             {"IsPrerequisity","Is prerequisity"}
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportConnector" /> class.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="owningItem">The owning item.</param>
        public ImportConnector(ConnectorDefinition definition, DiagramItem owningItem)
            : base(definition, ConnectorAlign.Left, owningItem)
        {
            InitializeComponent();
            Contract.Text = definition.GetProperty("Contract").Value;

            ConnectorTools.SetProperties(this, "Import info", ImportProperties);
            ConnectorTools.SetMessages(ErrorOutput, definition);
        }

        /// <summary>
        /// Gets the connect point.
        /// </summary>
        /// <value>The connect point.</value>
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
