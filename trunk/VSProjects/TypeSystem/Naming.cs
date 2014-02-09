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
        public static readonly char PathDelimiter = ';';

        public const string CtorName = "#ctor";

        public const string ClassCtorName = "#cctor";

        public const string GetterPrefix = "get_";

        public const string SetterPrefix = "set_";

        public static MethodID Method<CalledType>(string methodName, params Type[] paramTypes)
        {
            var path = typeof(CalledType).FullName + "." + methodName;
            return method(path, paramDescription(paramTypes), false);
        }

        public static MethodID Method(MethodBase method)
        {
            var paramTypes = (from param in method.GetParameters() select ParameterTypeInfo.From(param)).ToArray();

            return Naming.Method(TypeDescriptor.Create(method.DeclaringType), method.Name, method.IsVirtual, paramTypes);
        }

        public static MethodID Method(InstanceInfo declaringType, string methodName, bool needsDynamicResolution, params ParameterTypeInfo[] parameters)
        {
            var path = declaringType.TypeName + "." + methodName;

            return method(path, paramDescription(parameters), needsDynamicResolution);
        }

        public static MethodID GenericMethod(InstanceInfo declaringType, string methodName, bool needsDynamicResolution, TypeDescriptor[] methodTypeArguments, params ParameterTypeInfo[] parameters)
        {
            var typeNames = from argument in methodTypeArguments select argument.TypeName;
            var genericMethodName = methodName + "<" + string.Join(",", typeNames.ToArray()) + ">";

            var useGenericMethodName = methodTypeArguments.Length > 0;
            methodName = useGenericMethodName ? genericMethodName : methodName;

            return Method(declaringType, methodName, needsDynamicResolution, parameters);
        }

        private static MethodID method(string methodPath, string paramDescription, bool needsDynamicResolution)
        {
            return new MethodID(string.Format("{0}{1}{2}", methodPath, PathDelimiter, paramDescription), needsDynamicResolution);
        }

        private static string paramDescription(params object[] parameters)
        {
            var parCount = parameters == null ? 0 : parameters.Length;
            return parCount.ToString();
        }

        public static void GetParts(MethodID method, out string path, out string paramDescription)
        {
            var parts = method.MethodString.Split(new char[] { PathDelimiter }, 2);

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


            var nameStart = GetLastNonNestedPartDelimiter(methodPath);
            if (nameStart < 0)
                return null;

            return methodPath.Substring(nameStart + 1);
        }

        public static int GetLastNonNestedPartDelimiter(string methodPath)
        {
            var nesting = 0;
            for (var i = methodPath.Length - 1; i > 0; --i)
            {
                var ch = methodPath[i];
                switch (ch)
                {
                    //note that we are walking backward
                    case '<':
                        --nesting;
                        continue;
                    case '>':
                        ++nesting;
                        continue;
                    case '.':
                        if (nesting == 0)
                        {
                            return i;
                        }

                        continue;
                }
            }

            return -1;
        }

        public static string GetDeclaringType(MethodID method)
        {
            if (method == null)
                return null;

            string path, description;
            GetParts(method, out path, out description);

            return GetDeclaringType(path);
        }

        public static string GetDeclaringType(string methodPath)
        {
            if (methodPath == null)
                return null;

            var nameStart = GetLastNonNestedPartDelimiter(methodPath);
            if (nameStart < 0)
                return null;

            return methodPath.Substring(0, nameStart);
        }

        public static PathInfo GetMethodPath(MethodID method)
        {
            string path, paramDescr;
            Naming.GetParts(method, out path, out paramDescr);

            return new PathInfo(path);
        }

        public static MethodID ChangeDeclaringType(string typeName, MethodID changedMethod, bool needsDynamicResolving)
        {
            string path, description;
            GetParts(changedMethod, out path, out description);

            var methodName = GetMethodName(path);
            return method(typeName + "." + methodName, description, needsDynamicResolving);
        }

    }
}
