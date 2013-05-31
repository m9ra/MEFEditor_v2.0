using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution
{
    class Instance
    {        
        internal readonly VersionedName TypeName;
        private object literal;
        internal bool IsDirty { get; private set; }

        internal Instance(VersionedName typeName)
        {
            TypeName = typeName;
        }

        public Instance(object literal)
        {
            // TODO: Complete member initialization
            this.literal = literal;
        }
    }
}
