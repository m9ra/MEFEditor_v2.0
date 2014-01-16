using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

using System.Reflection;

namespace TypeSystem
{
    public class TypeMethodInfo
    {
        public readonly string MethodName;
        public readonly bool IsStatic;
        /// <summary>
        /// Determine that method cannot be invoked directly
        /// </summary>
        public readonly bool IsAbstract;
        public readonly ParameterTypeInfo[] Parameters;
        public readonly TypeDescriptor DeclaringType;
        public readonly TypeDescriptor ReturnType;
        public readonly MethodID MethodID;
        public readonly bool HasGenericParameters;


        public bool HasThis { get { return !IsStatic; } }

        public readonly PathInfo Path;

        public TypeMethodInfo(TypeDescriptor declaringType, string methodName, TypeDescriptor returnType, ParameterTypeInfo[] parameters, bool isStatic, bool hasGenericParams = false, bool isAbstract = false)
        {
            if (declaringType == null)
                throw new ArgumentNullException("thisType");

            DeclaringType = declaringType;

            MethodName = methodName;
            IsStatic = isStatic;
            IsAbstract = isAbstract;
            Parameters = parameters;
            ReturnType = returnType;
            MethodID = Naming.Method(declaringType, methodName, false, parameters);
            if (IsAbstract)
                MethodID = new MethodID(MethodID.MethodString, true);

            Path = new PathInfo(declaringType.TypeName + "." + methodName);
            HasGenericParameters = hasGenericParams;
        }

        public static TypeMethodInfo Create(MethodInfo method)
        {
            var paramsInfo = getParametersInfo(method);

            var declaringInfo = TypeDescriptor.Create(method.DeclaringType);
            var returnInfo = TypeDescriptor.Create(method.ReturnType);

            var isAbstract = method.IsAbstract;
            return new TypeMethodInfo(
                 declaringInfo, method.Name,
                 returnInfo, paramsInfo.ToArray(),
                 method.IsStatic, method.IsGenericMethodDefinition);
        }

        /// <summary>
        /// Get parameters info for given method base
        /// </summary>
        /// <param name="method">Base method which parameters will be created</param>
        /// <returns>Created parameters info</returns>
        private static IEnumerable<ParameterTypeInfo> getParametersInfo(MethodBase method)
        {
            var paramsInfo = new List<ParameterTypeInfo>();
            foreach (var param in method.GetParameters())
            {
                //TODO resolve generic arguments
                var paramType = TypeDescriptor.Create(param.ParameterType);
                var paramInfo = ParameterTypeInfo.From(param, paramType);
                paramsInfo.Add(paramInfo);
            }
            return paramsInfo;
        }

        public TypeMethodInfo MakeGenericMethod(PathInfo searchPath)
        {
            var translations = new Dictionary<string, string>();
            for (int i = 0; i < searchPath.GenericArgs.Count; ++i)
            {
                var genericParam = Path.GenericArgs[i];
                var genericArg = searchPath.GenericArgs[i];

                translations.Add(genericParam, genericArg);
            }

            return MakeGenericMethod(translations, searchPath);
        }

        public TypeMethodInfo MakeGenericMethod(Dictionary<string, string> translations, PathInfo searchPath = null)
        {
            var translatedName = MethodName;

            if (searchPath != null)
            {
                translatedName = searchPath.Name.Split('.').Last();
                if (searchPath.Signature != Path.Signature)
                    throw new NotSupportedException("Cannot make generic method from incompatible path info");
            }

            var translatedParams = new List<ParameterTypeInfo>();
            foreach (var parameter in Parameters)
            {
                var translatedType = translate(parameter.Type, translations);
                var translatedParam = parameter.MakeGeneric(translatedType);
                translatedParams.Add(translatedParam);
            }

            //TODO translate parameters, return type,..
            var generic = new TypeMethodInfo(
                translate(DeclaringType, translations), translatedName,
                translate(ReturnType, translations), translatedParams.ToArray(),
                IsStatic, false, IsAbstract);

            return generic;
        }

        private TypeDescriptor translate(InstanceInfo info, Dictionary<string, string> translations)
        {
            //TODO
            var name = info.TypeName;
            foreach (var pair in translations)
            {
                if (name == pair.Key)
                    name = pair.Value;

                name = name.Replace(pair.Key, pair.Value);
            }
            return TypeDescriptor.Create(name);
        }
    }
}
