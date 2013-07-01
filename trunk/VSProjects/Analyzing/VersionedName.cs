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


        public VersionedName(string name, int versionNumber)
        {
            VersionNumber = versionNumber;
            Name = name;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is VersionedName))
            {
                return false;
            }

            var o = (VersionedName)obj;
            return o.Name == Name && o.VersionNumber == VersionNumber;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() + VersionNumber;
        }
    }
}
