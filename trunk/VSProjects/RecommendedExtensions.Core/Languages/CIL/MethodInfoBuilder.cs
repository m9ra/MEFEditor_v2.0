using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil;

using MEFEditor.TypeSystem;

namespace RecommendedExtensions.Core.Languages.CIL
{
    /// <summary>
    /// Build TypeMethodInfo for given reference method. Declaring type and parameters
    /// are translated according to GenericInstance info.
    /// </summary>
    class MethodInfoBuilder
    {
        /// <summary>
        /// Available substitutions for generic parameters
        /// </summary>
        internal readonly TypeReferenceHelper TypeHelper;

        /// <summary>
        /// Type descriptor that is used as declaring type of builded TypeMethodInfo.
        /// </summary>
        internal TypeDescriptor DeclaringType;

        /// <summary>
        /// Type descriptor that is used as Return type of builded TypeMethodInfo.
        /// </summary>
        internal TypeDescriptor ReturnType;

        /// <summary>
        /// Determine that method needs dynamic resolving during runtime.
        /// Affects TypeMethodInfo IsAbstract attribute. For MethodDefinition is
        /// set by builder.
        /// </summary>
        internal bool NeedsDynamicResolving;

        /// <summary>
        /// Parameters used for builded TypeMethodInfo.
        /// </summary>
        internal List<ParameterTypeInfo> Parameters = new List<ParameterTypeInfo>();

        /// <summary>
        /// Type arguments used for builded TypeMethodInfo.
        /// </summary>
        internal List<TypeDescriptor> TypeArguments = new List<TypeDescriptor>();

        /// <summary>
        /// Name of method used for builded TypeMethedInfo.
        /// </summary>
        internal string MethodName;

        /// <summary>
        /// Determine that builded TypeMethodInfo describes static method.
        /// </summary>
        internal bool IsStatic;

        /// <summary>
        /// Determine offset of generic parameter, that is currently available. Is
        /// used for correct ordering accross method and type parameters
        /// </summary>
        private int _genericParamOffset = 0;

        /// <summary>
        /// Create builder for given method
        /// </summary>
        /// <param name="translatedMethod">Method which TypeMethodInfo is builded</param>
        internal MethodInfoBuilder(MethodReference translatedMethod, TypeReferenceHelper typeHelper)
        {
            TypeHelper = typeHelper;

            if (TypeHelper == null)
                throw new ArgumentNullException("typeHelper");

            applyGenericDeclaringType(translatedMethod.DeclaringType as GenericInstanceType);
            applyDeclaringType(translatedMethod.DeclaringType);

            applyMethodDefinition(translatedMethod as MethodDefinition);
            applyGenericMethod(translatedMethod as GenericInstanceMethod);
            applyMethod(translatedMethod);
        }

        /// <summary>
        /// Get type descriptor from given type. All available translation rules are applied
        /// </summary>
        /// <param name="type">Type which descriptor is created</param>
        /// <returns>Created type descriptor</returns>
        internal TypeDescriptor GetDescriptor(TypeReference type)
        {
            var result = TypeHelper.BuildDescriptor(type);
            return result;
        }

        /// <summary>
        /// Build TypeMethodInfo from current info in build properties
        /// </summary>
        /// <returns>Builded type method info</returns>
        internal TypeMethodInfo Build()
        {
            var result = new TypeMethodInfo(
                   DeclaringType,
                   MethodName,
                   ReturnType,
                   Parameters.ToArray(),
                   IsStatic,
                   TypeArguments.ToArray(),
                   NeedsDynamicResolving
                   );

            return result;
        }

        #region Build handlers

        /// <summary>
        /// Apply information available in declaring type reference
        /// </summary>
        /// <param name="typeReference">Type reference of declaring type</param>
        private void applyDeclaringType(TypeReference typeReference)
        {
            DeclaringType = GetDescriptor(typeReference);
        }

        /// <summary>
        /// Apply information available in declaring generic type
        /// </summary>
        /// <param name="genericType">Generic type instance of declaring type</param>
        private void applyGenericDeclaringType(GenericInstanceType genericType)
        {
            if (genericType == null)
                //there is no information available
                return;

            var arguments = genericType.GenericArguments;
            var parameters = genericType.ElementType.GenericParameters;

            applySubstitutions(arguments, parameters);
        }

        /// <summary>
        /// Apply information available in method reference
        /// </summary>
        /// <param name="methodReference">Method reference of builded method info</param>
        private void applyMethod(MethodReference methodReference)
        {
            //set default parameters
            foreach (var param in methodReference.Parameters)
            {
                var paramInfo = ParameterTypeInfo.Create(param.Name, GetDescriptor(param.ParameterType));
                Parameters.Add(paramInfo);
            }

            var name = methodReference.Name;
            switch (name)
            {
                case ".ctor":
                    name = Naming.CtorName;
                    break;
                case ".cctor":
                    name = Naming.ClassCtorName;
                    break;
            }

            //set default MethodName
            MethodName = name;

            //set default IsStatic
            IsStatic = !methodReference.HasThis;

            //set default ReturnType
            ReturnType = GetDescriptor(methodReference.ReturnType);

            //set method generic parameters if available
            var parameters = methodReference.GenericParameters;
            //TODO ensure that this doesnt colide with GenericInstanceMethods handling
            // in applyGenericMethod
            foreach (var par in parameters)
            {
                var parType = TypeHelper.BuildDescriptor(par);
                TypeArguments.Add(parType);
            }
        }

        /// <summary>
        /// Apply information available in generic instance method
        /// </summary>
        /// <param name="genericMethod">Generic instance method of builded method info</param>
        private void applyGenericMethod(GenericInstanceMethod genericMethod)
        {
            if (genericMethod == null)
                //there is no information on generic arguments available
                return;

            var arguments = genericMethod.GenericArguments;
            var parameters = genericMethod.ElementMethod.GenericParameters;

            applySubstitutions(arguments, parameters);

            foreach (var par in parameters)
            {
                var parType = TypeHelper.BuildDescriptor(par);
                TypeArguments.Add(parType);
            }
        }


        /// <summary>
        /// Apply information available in method definition
        /// </summary>
        /// <param name="genericMethod">Method definition of builded method info</param>
        private void applyMethodDefinition(MethodDefinition method)
        {
            if (method == null)
                //there is no information available
                return;

            NeedsDynamicResolving = method.IsAbstract;
        }

        /// <summary>
        /// Apply type parameters substitutions
        /// </summary>
        /// <param name="arguments">Type arguments</param>
        /// <param name="parameters"></param>
        private void applySubstitutions(IEnumerable<TypeReference> arguments, IEnumerable<GenericParameter> parameters)
        {
            var args = arguments.ToArray();
            var pars = parameters.ToArray();

            for (var i = 0; i < pars.Length; ++i)
            {
                var parameter = pars[i];

                var parameterDescriptor = TypeDescriptor.GetParameter(_genericParamOffset);
                ++_genericParamOffset;

                //These default descriptors are used below via GetDescriptor resolving algorithm
                TypeHelper.Substitutions[parameter] = parameterDescriptor;
            }

            for (var i = 0; i < args.Length; ++i)
            {
                var argument = args[i];
                var parameter = pars[i];

                var argumentDescriptor = GetDescriptor(argument);
                TypeHelper.Substitutions[parameter] = argumentDescriptor;
            }
        }

        #endregion

    }
}
