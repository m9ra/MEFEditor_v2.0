﻿using System;
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
        public ImportConnector(ConnectorDefinition definition,DiagramItem owningItem)
            :base(definition,owningItem)
        {
            InitializeComponent();
        }

        public override Point ConnectPoint
        {
            get {
                var res = new Point(-5, -5);
                res=this.TranslatePoint(res, Glyph);

                res = new Point(-res.X, -res.Y);
                return res; 
            }
        }
    }
}
