using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ExtensionPoints;
using System.ComponentModel.Composition;

namespace Example_extensions_DLL
{
    [Export(typeof(IContent))]
    public class TestContent:IContent
    {

        public string InnerHTML
        {
            get { return "Some interesting content from Example_extensions_dll.TestContent"; }
        }
    }

    [Export(typeof(IContent))]
    public class TestContent2 : IContent
    {
        public string InnerHTML
        {
            get { return "Some another interesting content from Example_extensions_dll.TestContent2"; }
        }
    }
}
