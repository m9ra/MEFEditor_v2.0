using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil;

using TypeSystem;

namespace AssemblyProviders.CIL
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
        internal TypeReferenceDirector TypeBuilder = new TypeReferenceDirector();

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
        internal MethodInfoBuilder(MethodReference translatedMethod)
        {
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
            var result = TypeBuilder.Build(type);
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

        private void applyDeclaringType(TypeReference type)
        {
            DeclaringType = GetDescriptor(type);
        }

        private void applyGenericDeclaringType(GenericInstanceType genericType)
        {
            if (genericType == null)
                //there is no information available
                return;

            var arguments = genericType.GenericArguments;
            var parameters = genericType.ElementType.GenericParameters;

            applySubstitutions(arguments, parameters);
        }

        private void applyMethod(MethodReference method)
        {
            //set default parameters
            foreach (var param in method.Parameters)
            {
                var paramInfo = ParameterTypeInfo.Create(param.Name, GetDescriptor(param.ParameterType));
                Parameters.Add(paramInfo);
            }

            var name = method.Name;
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
            IsStatic = !method.HasThis;

            //set default ReturnType
            ReturnType = GetDescriptor(method.ReturnType);

            //set method generic parameters if available
            var parameters = method.GenericParameters;
            //TODO ensure that this doesnt colide with GenericInstanceMethods handling
            // in applyGenericMethod
            foreach (var par in parameters)
            {
                var parType = TypeBuilder.Build(par);
                TypeArguments.Add(parType);
            }
        }

        private void applyGenericMethod(GenericInstanceMethod method)
        {
            if (method == null)
                //there is no information on generic arguments available
                return;

            var arguments = method.GenericArguments;
            var parameters = method.ElementMethod.GenericParameters;

            applySubstitutions(arguments, parameters);

            foreach (var par in parameters)
            {
                var parType = TypeBuilder.Build(par);
                TypeArguments.Add(parType);
            }
        }

        private void applyMethodDefinition(MethodDefinition method)
        {
            if (method == null)
                //there is no information available
                return;

            NeedsDynamicResolving = method.IsAbstract;
        }

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
                TypeBuilder.Substitutions[parameter] = parameterDescriptor;
            }

            for (var i = 0; i < args.Length; ++i)
            {
                var argument = args[i];
                var parameter = pars[i];

                var argumentDescriptor = GetDescriptor(argument);
                TypeBuilder.Substitutions[parameter] = argumentDescriptor;
            }
        }

        #endregion

    }
}
