using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor
{
    public class CompositionPointAttribute:Attribute
    {
        public readonly IEnumerable<object> Arguments;

        public CompositionPointAttribute(params object[] arguments)
        {
            Arguments = arguments;
        }
    }
}
