using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

namespace TypeSystem
{
    public class InheritanceChain
    {
        public readonly PathInfo Path;

        public readonly InstanceInfo Type;

        public readonly IEnumerable<InheritanceChain> SubTypes;

        internal InheritanceChain(InstanceInfo type, IEnumerable<InheritanceChain> subChains)
        {
            Type = type;
            Path = new PathInfo(type.TypeName);
            var filtered = from subChain in subChains where subChain != null select subChain;
            SubTypes = filtered.ToArray();
        }

        internal bool HasSubType(string targetTypeName)
        {
            if (targetTypeName == Type.TypeName)
            {
                return true;
            }

            foreach (var subType in SubTypes)
            {
                var hasSubType = subType.HasSubType(targetTypeName);
                if (hasSubType)
                    return true;
            }

            return false;
        }
    }
}
