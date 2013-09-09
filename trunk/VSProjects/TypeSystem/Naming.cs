using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using Analyzing;

namespace TypeSystem
{
    public static class Naming
    {

        public static MethodID Method<CalledType>(string methodName, params Type[] paramTypes)
        {
            return new MethodID(string.Format("{0}.{1}_{2}", typeof(CalledType).FullName, methodName, paramTypes.Length), false);
        }

        public static MethodID Method(InstanceInfo declaringType, string methodName, params ParameterInfo[] parameters)
        {
            var parCount = parameters == null ? 0 : parameters.Length;
            return new MethodID(string.Format("{0}.{1}_{2}",declaringType.TypeName,methodName,parCount),false);
        }
    }
}
