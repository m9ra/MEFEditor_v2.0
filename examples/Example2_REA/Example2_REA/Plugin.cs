using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ExtensibleApp.Interfaces;

using System.ComponentModel.Composition;

namespace Example2_REA
{
    [Export(typeof(IPlugin))]
    public class Plugin 
    {
        public string Name
        {
            get { return "SimplePlugin"; }
        }
    }
}
