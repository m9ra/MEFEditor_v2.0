using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ExtensionPoints;
using System.ComponentModel.Composition;


namespace Example_extensions_referenced
{
    [Export(typeof(IContent))]
    public class ReferencedContent : IContent
    {
        public string InnerHTML 
        {
            get { return "Content from referenced assembly"; } 
        }
    }
}
