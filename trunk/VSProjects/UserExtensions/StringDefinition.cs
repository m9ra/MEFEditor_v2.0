using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Composition;

using MEFEditor.TypeSystem.Runtime;

namespace UserExtensions
{
    [Export(typeof(RuntimeTypeDefinition))]
    public class StringDefinition : DirectTypeDefinition<string>
    {
        public string _method_ToString()
        {
            //v ThisValue máme hodnotu reprezentovanou 
            //právě volanou instancí, vytvoříme
            //proto upravenou hodnotu
            var result = "Changed: " + ThisValue;
            //a tuto hodnotu vrátíme
            return result;
        }
    }
}
