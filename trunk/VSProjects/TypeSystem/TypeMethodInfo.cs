using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

using System.Reflection;

namespace TypeSystem
{
    /// <summary>
    /// Class used for describing methods within whole TypeSystem. Usable for parsers, compilers,...
    /// 
    /// All Assembly providers has to produce this kind for info for every requested method
    /// </summary>
    public class TypeMethodInfo
    {
        /// <summary>
        /// Name of described method (without generic arguments)
        /// </summary>
        public readonly string MethodName;

        /// <summary>
        /// Parameters defined for described method
        /// </summary>
        public readonly ParameterTypeInfo[] Parameters;

        /// <summary>
        /// Type arguments that are contained within method
        /// </summary>
        public readonly TypeDescriptor[] MethodTypeArguments;

        /// <summary>
        /// Type where described method is declared
        /// </summary>
        public readonly TypeDescriptor DeclaringType;

        /// <summary>
        /// Return type of described method
        /// </summary>
        public readonly TypeDescriptor ReturnType;

        /// <summary>
        /// Determine that method is static (shared through application domain)
        /// </summary>
        public readonly bool IsStatic;

        /// <summary>
        /// Determine that method expects "this object"
        /// </summary>
        public bool HasThis { get { return !IsStatic; } }

        /// <summary>
        /// Determine that method cannot be invoked directly
        /// </summary>
        public readonly bool IsAbstract;

        /// <summary>
        /// Determine that method has generic parameters
        /// </summary>
        public readonly bool HasGenericParameters;

        /// <summary>
        /// ID for described method
        /// </summary>
        public readonly MethodID MethodID;

        /// <summary>
        /// Path for described method
        /// </summary>
        public readonly PathInfo Path;

        public TypeMethodInfo(TypeDescriptor declaringType, string methodName, TypeDescriptor returnType, ParameterTypeInfo[] parameters, bool isStatic, TypeDescriptor[] methodTypeArguments, bool isAbstract = false)
        {
            if (declaringType == null)
                throw new ArgumentNullException("declaringType");

            DeclaringType = declaringType;
            MethodName = methodName;
            IsStatic = isStatic;
            IsAbstract = isAbstract;
            Parameters = parameters;
            MethodTypeArguments = methodTypeArguments;
            ReturnType = returnType;

            // Create ID for described method
            MethodID = Naming.GenericMethod(declaringType, methodName, false, methodTypeArguments, parameters);
            if (IsAbstract)
                MethodID = new MethodID(MethodID.MethodString, true);

            // Create path info for method
            Path = Naming.GetMethodPath(MethodID);

            HasGenericParameters = DeclaringType.HasParameters;
            HasGenericParameters |=  ReturnType.IsParameter;
            foreach (var arg in methodTypeArguments)
            {
                HasGenericParameters |= arg.IsParameter;
            }
        }

        /// <summary>
        /// Creates type method info from givem method info
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static TypeMethodInfo Create(MethodInfo method)
        {
            var paramsInfo = getParametersInfo(method);
            var genericArgsInfo = getTypeArguments(method);

            var declaringInfo = TypeDescriptor.Create(method.DeclaringType);
            var returnInfo = TypeDescriptor.Create(method.ReturnType);

            var isAbstract = method.IsAbstract;
            return new TypeMethodInfo(
                 declaringInfo, method.Name,
                 returnInfo, paramsInfo,
                 method.IsStatic, genericArgsInfo);
        }

        #region API for deriving generic methods

        /// <summary>
        /// Creates generic method info from current definition according to path
        /// </summary>
        /// <param name="path">Path where parameters substitutions are defined</param>
        /// <returns>Maked method info</returns>
        public TypeMethodInfo MakeGenericMethod(PathInfo path)
        {
            if (path.ShortSignature!= Path.ShortSignature)
                    throw new NotSupportedException("Cannot make generic method from incompatible path info");


            var translations = new Dictionary<string, string>();
            for (int i = 0; i < path.GenericArgs.Count; ++i)
            {
                var genericParam = Path.GenericArgs[i];
                var genericArg = path.GenericArgs[i];

                translations.Add(genericParam, genericArg);
            }

            return MakeGenericMethod(translations);
        }

        /// <summary>
        /// Creates generic method info from current definition according to given translations and path
        /// </summary>
        /// <param name="translations">Translations defining generic parameters substitutions</param>
        /// <returns>Maked method info</returns>
        public TypeMethodInfo MakeGenericMethod(Dictionary<string, string> translations)
        {
            var translatedName=translateString(MethodName, translations);

            var translatedParams = new List<ParameterTypeInfo>();
            foreach (var parameter in Parameters)
            {
                var translatedType = translate(parameter.Type, translations);
                var translatedParam = parameter.MakeGeneric(translatedType);
                translatedParams.Add(translatedParam);
            }

            var methodTypeArguments = new List<TypeDescriptor>(MethodTypeArguments.Length);
            foreach (var argument in MethodTypeArguments)
            {
                var translatedType = translate(argument, translations);
                methodTypeArguments.Add(translatedType);
            }

            var generic = new TypeMethodInfo(
                translate(DeclaringType, translations), translatedName,
                translate(ReturnType, translations), translatedParams.ToArray(),
                IsStatic, methodTypeArguments.ToArray(), IsAbstract);

            return generic;
        }

        #endregion

        /// <summary>
        /// Get parameters info for given method base
        /// </summary>
        /// <param name="method">Base method which parameters will be created</param>
        /// <returns>Created parameters info</returns>
        private static ParameterTypeInfo[] getParametersInfo(MethodBase method)
        {
            var paramsInfo = new List<ParameterTypeInfo>();
            foreach (var param in method.GetParameters())
            {
                //TODO resolve generic arguments
                var paramType = TypeDescriptor.Create(param.ParameterType);
                var paramInfo = ParameterTypeInfo.From(param, paramType);
                paramsInfo.Add(paramInfo);
            }
            return paramsInfo.ToArray();
        }

        private static TypeDescriptor[] getTypeArguments(MethodInfo method)
        {
            var result = new List<TypeDescriptor>();
            foreach (var arg in method.GetGenericArguments())
            {
                var typeArg = TypeDescriptor.Create(arg);
                result.Add(typeArg);
            }

            return result.ToArray();
        }


        /// <summary>
        /// Translates given info with available translations
        /// </summary>
        /// <param name="translatedDescriptor">Descriptor that will be translated</param>
        /// <param name="translations">Available translations</param>
        /// <returns>Translated type descriptor</returns>
        private TypeDescriptor translate(TypeDescriptor translatedDescriptor, Dictionary<string, string> translations)
        {
            //TODO TypeDescriptor support for translations
            var name = translatedDescriptor.TypeName;

            name=translateString(name, translations);

            return TypeDescriptor.Create(name);
        }


        /// <summary>
        /// TODO refactor out
        /// </summary>
        /// <param name="name"></param>
        /// <param name="translations"></param>
        /// <returns></returns>
        private string translateString(string name, Dictionary<string, string> translations)
        {
            foreach (var pair in translations)
            {
                name = name.Replace(pair.Key, pair.Value);
            }

            return name;
        }
    }
}
