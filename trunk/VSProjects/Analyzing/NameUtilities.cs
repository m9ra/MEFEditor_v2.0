using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace Analyzing
{
    public static class Name
    {
        public static VersionedName FromMethod(MethodDescription description, int versionNumber = 0)
        {
            //TODO proper name creating
            var codedName = string.Format("{0}/{1}/{2}", description.ThisType.Fullname, description.MethodName, description.Parameters.Length);

            return new VersionedName(codedName, versionNumber);
        }

        internal static VersionedName FromMethod(MethodInfo method,int versionNumber=0)
        {
            var codedName = string.Format("{0}/{1}/{2}", method.DeclaringType.FullName, method.Name, method.GetParameters().Length);

            return new VersionedName(codedName, versionNumber);
        }
    }
}
