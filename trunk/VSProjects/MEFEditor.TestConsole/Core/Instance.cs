using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeExperiments.Core
{
    internal class Instance
    {
        private InternalType _type;

        public bool IsShared{get;private set;}

        public Instance(InternalType type)
        {
            _type = type;
        }

        internal InternalType GetInternalType()
        {
            return _type;
        }

        
    }
}
