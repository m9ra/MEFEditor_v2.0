using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EnvDTE;
using EnvDTE80;

using MEFEditor.TypeSystem;

using RecommendedExtensions.Core.AssemblyProviders.ProjectAssembly.Traversing;

namespace RecommendedExtensions.Core.AssemblyProviders.ProjectAssembly.MethodBuilding
{
    /// <summary>
    /// Builder of <see cref="TypeMethodInfo" /> objects from <see cref="CodeElement" /> definitions.
    /// </summary>
    public class MethodInfoBuilder : CodeElementVisitor
    {
        /// <summary>
        /// Result of build.
        /// </summary>
        private TypeMethodInfo _result;

        /// <summary>
        /// Assembly declaring <see cref="CodeElement" /> definitions.
        /// </summary>
        /// <value>The declaring assembly.</value>
        protected VsProjectAssembly DeclaringAssembly { get; private set; }

        /// <summary>
        /// Name that is requried for method that will be built.
        /// </summary>
        /// <value>The name of the required.</value>
        protected string RequiredName { get; private set; }

        /// <summary>
        /// Initialize new instance of builder of <see cref="TypeMethodInfo" />.
        /// </summary>
        public MethodInfoBuilder()
        {
            RecursiveVisit = false;
        }

        /// <summary>
        /// Initialize builder with given assembly.
        /// </summary>
        /// <param name="declaringAssembly">Assembly where are declared definitions to built.</param>
        /// <exception cref="System.ArgumentNullException">declaringAssembly</exception>
        internal void Initialize(VsProjectAssembly declaringAssembly)
        {
            if (declaringAssembly == null)
                throw new ArgumentNullException("declaringAssembly");

            DeclaringAssembly = declaringAssembly;
        }

        /// <summary>
        /// Build <see cref="TypeMethodInfo" /> from given element.
        /// </summary>
        /// <param name="element">Method definition element.</param>
        /// <param name="requiredName">Name that is required from builded info.</param>
        /// <returns>Built method.</returns>
        public TypeMethodInfo Build(CodeElement element, string requiredName)
        {
            TypeMethodInfo result;
            try
            {
                _result = null;
                RequiredName = requiredName;
                VisitElement(element);
            }
            finally
            {
                RequiredName = null;
                result = _result;
                _result = null;
            }

            return result;
        }

        /// <summary>
        /// Report result of build.
        /// </summary>
        /// <param name="info">Reported result.</param>
        protected void Result(TypeMethodInfo info)
        {
            _result = info;
        }

        #region Concrete builders

        /// <summary>
        /// Creates <see cref="TypeMethodInfo" /> for given element.
        /// </summary>
        /// <param name="element">Element which <see cref="TypeMethodInfo" /> is created.</param>
        /// <returns>Created <see cref="TypeMethodInfo" />.</returns>
        protected TypeMethodInfo BuildFrom(CodeFunction element)
        {
            //collect information from element - note, every call working with element may fail with exception, because of VS doesn't provide determinism
            var name = RequiredName;
            var isShared = element.IsShared;
            var isAbstract = element.MustImplement || element.IsVirtual();
            var declaringType = CreateDescriptor(element.DeclaringType());
            var returnType = CreateDescriptor(element.Type);
            var parameters = CreateParametersInfo(element.Parameters);

            //Methods cannot have generic arguments (only parameters, that are contained within path)
            var methodTypeArguments = TypeDescriptor.NoDescriptors;

            //create result according to collected information
            var methodInfo = new TypeMethodInfo(
                declaringType, name, returnType, parameters,
                isShared, methodTypeArguments, isAbstract
                );

            return methodInfo;
        }

        /// <summary>
        /// Creates <see cref="TypeMethodInfo" /> for given element.
        /// </summary>
        /// <param name="element">Element which <see cref="TypeMethodInfo" /> is created.</param>
        /// <returns>Created <see cref="TypeMethodInfo" />.</returns>
        protected TypeMethodInfo BuildFrom(CodeProperty element)
        {
            //translate name according to naming conventions of type system
            var buildGetter = RequiredName.StartsWith(Naming.GetterPrefix);
            var namePrefix = buildGetter ? Naming.GetterPrefix : Naming.SetterPrefix;

            var property2 = element as CodeProperty2;
            var isShared = property2 != null && property2.IsShared;

            var method = buildGetter ? element.Getter : element.Setter;
            var isAbstract = method == null || method.MustImplement;

            var declaringTypeNode = element.DeclaringClass();
            var declaringType = CreateDescriptor(declaringTypeNode);
            var variableType = CreateDescriptor(element.Type);

            //properties cannot have type arguments
            var methodTypeArguments = TypeDescriptor.NoDescriptors;

            TypeDescriptor returnType;
            ParameterTypeInfo[] parameters;
            if (buildGetter)
            {
                returnType = variableType;
                parameters = ParameterTypeInfo.NoParams;
            }
            else
            {
                returnType = TypeDescriptor.Void;
                parameters = new[] { ParameterTypeInfo.Create("value", variableType) };
            }

            var isIndexer = RequiredName == Naming.IndexerSetter || RequiredName == Naming.IndexerGetter;
            if (isIndexer)
            {                
                var indexParameters = CreateParametersInfo(method.Parameters);
                parameters = indexParameters.Concat(parameters).ToArray();
            }

            var methodInfo = new TypeMethodInfo(
                declaringType, RequiredName, returnType, parameters,
                isShared, methodTypeArguments, isAbstract
                );

            return methodInfo;
        }

        /// <summary>
        /// Creates <see cref="TypeMethodInfo" /> for given element.
        /// </summary>
        /// <param name="element">Element which <see cref="TypeMethodInfo" /> is created.</param>
        /// <returns>Created <see cref="TypeMethodInfo" />.</returns>
        internal TypeMethodInfo BuildFrom(CodeVariable element)
        {
            var isShared = element.IsShared;
            var isAbstract = false; //variables cannot be abstract
            var declaringType = CreateDescriptor(element.Parent as CodeClass);
            var variableType = CreateDescriptor(element.Type);

            //variables cannot have type arguments
            var methodTypeArguments = TypeDescriptor.NoDescriptors;

            TypeDescriptor returnType;
            ParameterTypeInfo[] parameters;

            var buildGetter = RequiredName.StartsWith(Naming.GetterPrefix);
            if (buildGetter)
            {
                returnType = variableType;
                parameters = ParameterTypeInfo.NoParams;
            }
            else
            {
                returnType = TypeDescriptor.Void;
                parameters = new[] { ParameterTypeInfo.Create("value", variableType) };
            }

            var methodInfo = new TypeMethodInfo(
                declaringType, RequiredName, returnType, parameters,
                isShared, methodTypeArguments, isAbstract
                );

            return methodInfo;
        }
        
        #endregion

        /// <summary>
        /// Creates parameter's info from given <see cref="CodeElements" /> describing method parameters.
        /// </summary>
        /// <param name="parameters">Method parameters described by <see cref="CodeElements" />.</param>
        /// <returns>Created parameter's info.</returns>
        public ParameterTypeInfo[] CreateParametersInfo(CodeElements parameters)
        {
            var result = new List<ParameterTypeInfo>();
            foreach (CodeParameter parameter in parameters)
            {
                var paramName = parameter.Name;
                var paramType = CreateDescriptor(parameter.Type);

                //TODO: default values handling
                var parameterInfo = ParameterTypeInfo.Create(paramName, paramType);
                result.Add(parameterInfo);
            }

            return result.ToArray();
        }
        
        #region Type descriptor building routines

        /// <summary>
        /// Create <see cref="TypeDescriptor" /> from given element.
        /// </summary>
        /// <param name="element">Element definition of type.</param>
        /// <returns>Created <see cref="TypeDescriptor" />.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public TypeDescriptor CreateDescriptor(CodeElement element)
        {
            if (element is CodeTypeRef)
                return CreateDescriptor(element as CodeTypeRef);

            if (element is CodeClass)
                return CreateDescriptor(element as CodeClass);

            if (element is CodeInterface)
                return CreateDescriptor(element as CodeInterface);

            return null;
        }

        /// <summary>
        /// Create <see cref="TypeDescriptor" /> from given typeReference.
        /// </summary>
        /// <param name="typeReference">Reference on type.</param>
        /// <returns>Created <see cref="TypeDescriptor" />.</returns>
        public TypeDescriptor CreateDescriptor(CodeTypeRef typeReference)
        {
            var fullname = typeReference.AsFullName;
            if (fullname == "")
                fullname = typeReference.AsString;

            if (fullname == "")
                return TypeDescriptor.Void;

            return ConvertToDescriptor(fullname, false);
        }

        /// <summary>
        /// Create <see cref="TypeDescriptor" /> from given type.
        /// </summary>
        /// <param name="type">Type representation.</param>
        /// <returns>Created <see cref="TypeDescriptor" />.</returns>
        public TypeDescriptor CreateDescriptor(CodeType type)
        {
            var fullname = type.FullName;
            if (fullname == "")
                return TypeDescriptor.Void;

            return ConvertToDescriptor(fullname, false);
        }

        /// <summary>
        /// Creates <see cref="TypeDescriptor" /> from given typeNode.
        /// </summary>
        /// <param name="typeNode">Type node which descriptor is created.</param>
        /// <returns>Created <see cref="TypeDescriptor" />.</returns>
        public TypeDescriptor CreateDescriptor(CodeClass typeNode)
        {
            var fullname = typeNode.FullName;

            return ConvertToDescriptor(fullname, true);
        }


        /// <summary>
        /// Creates <see cref="TypeDescriptor" /> from given typeNode.
        /// </summary>
        /// <param name="typeNode">Type node which descriptor is created.</param>
        /// <returns>Created <see cref="TypeDescriptor" />.</returns>
        public TypeDescriptor CreateDescriptor(CodeInterface typeNode)
        {
            var fullname = typeNode.FullName;

            return ConvertToDescriptor(fullname, true);
        }

        /// <summary>
        /// Converts fullnames between <see cref="CodeModel" /> representation and TypeSystem typeName.
        /// </summary>
        /// <param name="fullname">Fullname of element from <see cref="CodeModel" />.</param>
        /// <param name="hasOnlyGenericArguments">Determine that fullname belongs to compile time element
        /// (like <see cref="CodeClass" /> or <see cref="CodeInterface" />) or not.</param>
        /// <returns>TypeDescriptor.</returns>
        public TypeDescriptor ConvertToDescriptor(string fullname, bool hasOnlyGenericArguments)
        {
            //every fullname collected from assembly is compile time - thus all generic arguments are parameters
            var convertedName = hasOnlyGenericArguments ? fullname.Replace("<", "<@").Replace(",", ",@") : fullname;

            convertedName = arrayResolver(convertedName);
            convertedName = TypeDescriptor.TranslatePath(convertedName, arrayResolver);

            var descriptor = TypeDescriptor.Create(convertedName);

            return descriptor;
        }

        /// <summary>
        /// Arrays the resolver.
        /// </summary>
        /// <param name="genericParameter">The generic parameter.</param>
        /// <returns>System.String.</returns>
        private string arrayResolver(string genericParameter)
        {
            if (genericParameter.EndsWith("[]"))
            {
                var itemType = genericParameter.Substring(0, genericParameter.Length - 2);
                itemType = DeclaringAssembly.TranslatePath(itemType);
                genericParameter = string.Format("Array<{0},1>", itemType);
            }

            return genericParameter;
        }

        #endregion

        #region Visitor overrides

        /// <summary>
        /// Visit given element.
        /// </summary>
        /// <param name="e">Element to visit.</param>
        /// <inheritdoc />
        public override void VisitFunction(CodeFunction2 e)
        {
            Result(BuildFrom(e));
        }

        /// <summary>
        /// Visit given element.
        /// </summary>
        /// <param name="e">Element to visit.</param>
        /// <inheritdoc />
        public override void VisitVariable(CodeVariable e)
        {
            Result(BuildFrom(e));
        }

        /// <summary>
        /// Visit given element.
        /// </summary>
        /// <param name="e">Element to visit.</param>
        /// <inheritdoc />
        public override void VisitProperty(CodeProperty e)
        {
            Result(BuildFrom(e));
        }

        #endregion

    }
}
