using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;

using System.Reflection;

namespace MEFEditor.TypeSystem
{
    /// <summary>
    /// Class used for describing methods within whole TypeSystem. Usable for parsers, compilers,...
    /// All Assembly providers has to produce this kind for info for every requested method.
    /// </summary>
    public class TypeMethodInfo
    {
        /// <summary>
        /// Name of described method (without generic arguments).
        /// </summary>
        public readonly string MethodName;

        /// <summary>
        /// Parameters defined for described method.
        /// </summary>
        public readonly ParameterTypeInfo[] Parameters;

        /// <summary>
        /// Type arguments that are contained within method.
        /// </summary>
        public readonly TypeDescriptor[] MethodTypeArguments;

        /// <summary>
        /// Type where described method is declared.
        /// </summary>
        public readonly TypeDescriptor DeclaringType;

        /// <summary>
        /// Return type of described method.
        /// </summary>
        public readonly TypeDescriptor ReturnType;

        /// <summary>
        /// Determine that method is static (shared through application domain).
        /// </summary>
        public readonly bool IsStatic;

        /// <summary>
        /// Determine that method expects "this object".
        /// </summary>
        /// <value><c>true</c> if this instance has this; otherwise, <c>false</c>.</value>
        public bool HasThis { get { return !IsStatic; } }

        /// <summary>
        /// Determine that method cannot be invoked directly.
        /// </summary>
        public readonly bool IsAbstract;

        /// <summary>
        /// Determine that method has generic parameters.
        /// </summary>
        public readonly bool HasGenericParameters;

        /// <summary>
        /// ID for described method.
        /// </summary>
        public readonly MethodID MethodID;

        /// <summary>
        /// Path for described method.
        /// </summary>
        public readonly PathInfo Path;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeMethodInfo"/> class.
        /// </summary>
        /// <param name="declaringType">Type declaring the method.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="returnType">Type of the return value.</param>
        /// <param name="parameters">The method's parameters.</param>
        /// <param name="isStatic">if set to <c>true</c> method is static.</param>
        /// <param name="methodTypeArguments">The method type arguments.</param>
        /// <param name="isAbstract">if set to <c>true</c> method is abstract.</param>
        /// <exception cref="System.ArgumentNullException">declaringType</exception>
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
            HasGenericParameters |= ReturnType.IsParameter;
            foreach (var arg in methodTypeArguments)
            {
                HasGenericParameters |= arg.IsParameter;
            }
        }

        /// <summary>
        /// Creates type method info from givem method info.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>TypeMethodInfo.</returns>
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
        /// Creates generic method info from current definition according to path.
        /// </summary>
        /// <param name="path">Path where parameters substitutions are defined.</param>
        /// <returns>Maked method info.</returns>
        /// <exception cref="System.NotSupportedException">Cannot make generic method from incompatible path info</exception>
        public TypeMethodInfo MakeGenericMethod(PathInfo path)
        {
            if (path.ShortSignature != Path.ShortSignature)
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
        /// Creates generic method info from current definition according to given translations and path.
        /// </summary>
        /// <param name="translations">Translations defining generic parameters substitutions.</param>
        /// <returns>Maked method info.</returns>
        public TypeMethodInfo MakeGenericMethod(Dictionary<string, string> translations)
        {
            var translatedName = TypeDescriptor.TranslatePath(MethodName, translations);

            var translatedParams = new List<ParameterTypeInfo>();
            foreach (var parameter in Parameters)
            {
                var translatedType = parameter.Type.MakeGeneric(translations);
                var translatedParam = parameter.MakeGeneric(translatedType);
                translatedParams.Add(translatedParam);
            }

            var methodTypeArguments = new List<TypeDescriptor>(MethodTypeArguments.Length);
            foreach (var argument in MethodTypeArguments)
            {
                var translatedType = argument.MakeGeneric(translations);
                methodTypeArguments.Add(translatedType);
            }

            var generic = new TypeMethodInfo(
                DeclaringType.MakeGeneric(translations), translatedName,
                ReturnType.MakeGeneric(translations), translatedParams.ToArray(),
                IsStatic, methodTypeArguments.ToArray(), IsAbstract);

            return generic;
        }

        #endregion

        /// <summary>
        /// Get parameters info for given method base.
        /// </summary>
        /// <param name="method">Base method which parameters will be created.</param>
        /// <returns>Created parameters info.</returns>
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

        /// <summary>
        /// Gets the type arguments of method.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>TypeDescriptor[].</returns>
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
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return "[TypeMethodInfo]" + MethodID.MethodString;
        }
    }
}
