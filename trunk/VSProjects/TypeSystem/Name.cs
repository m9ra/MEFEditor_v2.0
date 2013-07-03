using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using Analyzing;

namespace TypeSystem
{
    static class Name
    {
        internal static VersionedName From(MethodInfo method, int version=0)
        {
            var name = string.Format("{0}.{1}_{2}", method.DeclaringType.FullName, method.Name, method.GetParameters().Count());
            return new VersionedName(name,version);
        }

        internal static VersionedName From(MethodID method,InstanceInfo[] info, int version = 0)
        {
            //TODO this resolving is not sufficient
            var name = string.Format("{0}.{1}_{2}", info[0].TypeName,method.MethodName, info.Length - 1);
            return new VersionedName(name, version);
        }

        
    }
}
