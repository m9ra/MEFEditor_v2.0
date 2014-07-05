using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ExtensionPoints;
using System.ComponentModel.Composition;

namespace Example_extensions_referenced
{    
    [Export(typeof(ILayout))] 
    public class SimpleLayout : ILayout
    {
        [ImportMany(typeof(IContent))]
        IContent[] Contents;

        public string GetPageHTML
        {
            get
            {
                StringBuilder body = new StringBuilder();

                for (int i = 0; i < Contents.Length; ++i)
                {
                    body.AppendFormat(@"
<div>
    <h2>Content number {0}</h2>
    <br>
    {1}    
</div>
", i+1, Contents[i].InnerHTML);

                }

                return string.Format(@"
<html>
    <head>
        <title>Simple Layout page</title>
    </head>
    <body>
    {0}
    </body>
</html>
                        ",body);
            }
        }
    }


    [Export(typeof(ILayout))]
    public class WrongLayout
    {
    }
}
