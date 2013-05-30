using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing
{
    public struct VersionedName
    {
        /// <summary>
        /// Determine current version of object (every change on object has to increase version number)
        /// </summary>
        public readonly int VersionNumber;
        /// <summary>
        /// Determine fully qualified name of object
        /// </summary>
        public readonly string Name;

        //TODO override hashcode/equals/..
    }
}
