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
            return method(path, paramDescription(paramTypes), false);
        }

        public static MethodID Method(InstanceInfo declaringType, string methodName, params ParameterTypeInfo[] parameters)
        {
            var path = declaringType.TypeName + "." + methodName;

            return method(path, paramDescription(parameters), false);
        }

        private static MethodID method(string methodPath, string paramDescription, bool needsDynamicResolution)
        {
            return new MethodID(string.Format("{0}{1}{2}", methodPath, PathSplit, paramDescription), needsDynamicResolution);
        }

        private static string paramDescription(params object[] parameters)
        {
            var parCount = parameters == null ? 0 : parameters.Length;
            return parCount.ToString();
        }

        internal static void GetParts(MethodID method, out string path, out string paramDescription)
        {
            var parts = method.MethodString.Split(new char[] { PathSplit }, 2);

            path = parts[0];
            paramDescription = parts[1];
        }

        public static string GetMethodName(MethodID method)
        {
            if (method == null)
                return null;

            string path, description;
            GetParts(method, out path, out description);

            return GetMethodName(path);
        }

        public static string GetMethodName(string methodPath)
        {
            if (methodPath == null)
                return null;

            var nameStart = methodPath.LastIndexOf('.');
            if (nameStart < 0)
                return null;

            return methodPath.Substring(nameStart + 1);
        }

        public static MethodID ChangeDeclaringType(InstanceInfo type, MethodID changedMethod, bool needsDynamicResolving)
        {
            string path, description;
            GetParts(changedMethod, out path, out description);

            var methodName = GetMethodName(path);
            return method(type.TypeName + "." + methodName, description, needsDynamicResolving);
        }
    }
}
