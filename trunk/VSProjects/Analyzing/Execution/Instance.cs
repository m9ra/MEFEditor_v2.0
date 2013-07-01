using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution
{
    public class Instance
    {            
        internal bool IsDirty { get; private set; }

        public object DirectValue { get; private set; }

 

        public Instance(object directValue)
        {
            DirectValue = directValue;
        }        
    }
}
