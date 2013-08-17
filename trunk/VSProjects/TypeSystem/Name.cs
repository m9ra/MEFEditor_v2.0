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
            var name = string.Format("{0}_{1}",method.Name, method.GetParameters().Count(),method.DeclaringType.FullName);
            return new VersionedName(name,version);
        }

        internal static VersionedName From(MethodID method,InstanceInfo[] info, int version = 0)
        {
            //TODO this resolving is not sufficient
            var name = string.Format("{0}_{1}", method.MethodName, info.Length - 1,info[0].TypeName);
            return new VersionedName(name, version);
        }

        
    }
}
