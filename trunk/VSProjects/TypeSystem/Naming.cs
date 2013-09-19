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
        public static readonly char PathSplit = ';';

        public static MethodID Method<CalledType>(string methodName, params Type[] paramTypes)
        {
            var path = typeof(CalledType).FullName + "." + methodName;
            return method(path, paramDescription(paramTypes));
        }

        public static MethodID Method(InstanceInfo declaringType, string methodName, params ParameterTypeInfo[] parameters)
        {
            var path = declaringType.TypeName + "." + methodName;

            return method(path, paramDescription(parameters));
        }

        private static MethodID method(string methodPath, string paramDescription)
        {
            return new MethodID(string.Format("{0}{1}{2}", methodPath, PathSplit, paramDescription), false);
        }

        private static string paramDescription(params object[] parameters)
        {
            var parCount = parameters == null ? 0 : parameters.Length;
            return parCount.ToString();
        }

        internal static void GetParts(MethodID method, out string path, out string paramDescription)
        {
            var parts = method.MethodName.Split(new char[] { PathSplit }, 2);

            path = parts[0];
            paramDescription = parts[1];
        }
    }
}
