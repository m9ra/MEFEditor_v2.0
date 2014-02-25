using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnvDTE;
using EnvDTE80;

using TypeSystem;

using AssemblyProviders.ProjectAssembly.Traversing;

namespace AssemblyProviders.ProjectAssembly.MethodBuilding
{
    /// <summary>
    /// Builder of <see cref="MethodItem"/> objects from <see cref="CodeElement"/> definitions.
    /// </summary>
    class MethodBuilder : CodeElementVisitor
    {
        /// <summary>
        /// Stores result of method build process
        /// </summary>
        private MethodItem _result;

        /// <summary>
        /// Assembly where builded method has been declared
        /// </summary>
        private readonly VsProjectAssembly _declaringAssembly;

        /// <summary>
        /// Hide constructor - Static Build method should be used
        /// </summary>
        private MethodBuilder(VsProjectAssembly declaringAssembly)
        {
            _declaringAssembly = declaringAssembly;
        }

        #region Method building API

        /// <summary>
        /// Build <see cref="MethodItem"/> from given element.
        /// </summary>
        /// <param name="element">Method definition element</param>
        /// <returns>Builded method</returns>
        internal static MethodItem Build(CodeElement element, VsProjectAssembly declaringAssembly)
        {
            var builder = new MethodBuilder(declaringAssembly);

            builder.VisitElement(element);

            var result = builder._result;

            if (result == null)
                throwNotSupportedElement(element);

            return result;
        }

        /// <summary>
        /// Build <see cref="MethodItem"/> from given function element.
        /// </summary>
        /// <param name="element">Method definition element</param>
        /// <returns>Builded method</returns>
        internal static MethodItem BuildFrom(CodeFunction element, VsProjectAssembly declaringAssembly)
        {
            var sourceCode = GetSourceCode(element);
            var methodInfo = CreateMethodInfo(element);

            var fullname = element.FullName;
            var genericPath = new PathInfo(fullname);
            var activation = new ParsingActivation(sourceCode, methodInfo, genericPath.GenericArgs);

            var generator = new SourceMethodGenerator(activation, declaringAssembly.ParsingProvider);

            var item = new MethodItem(generator, methodInfo);
            return item;
        }

        /// <summary>
        /// Creates <see cref="TypeMethodInfo"/> for given element
        /// </summary>
        /// <param name="element">Element which <see cref="TypeMethodInfo"/> is created</param>
        /// <returns>Created <see cref="TypeMethodInfo"/></returns>
        internal static TypeMethodInfo CreateMethodInfo(CodeFunction element)
        {
            //collect information from element - note, every call working with element may fail with exception, because of VS doesn't provide determinism
            var name = element.Name;
            var isConstructor = element.FunctionKind == vsCMFunction.vsCMFunctionConstructor;
            var isShared = element.IsShared;
            var isAbstract = element.MustImplement;
            var declaringType = CreateDescriptor(element.Parent);
            var returnType = CreateDescriptor(element.Type);
            var parameters = CreateParametersInfo(element.Parameters);
            //there are no generic arguments on method definition
            var methodTypeArguments = TypeDescriptor.NoDescriptors;

            if (isConstructor)
            {
                //repair name according to naming conventions
                name = isShared ? Naming.ClassCtorName : Naming.CtorName;
            }

            //create result according to collected information
            var methodInfo = new TypeMethodInfo(
                declaringType, name, returnType, parameters,
                isShared, methodTypeArguments, isAbstract
                );

            return methodInfo;
        }

        /// <summary>
        /// Creates parameter's info from given <see cref="CodeElements"/> describing method parameters
        /// </summary>
        /// <param name="parameters">Method parameters described by <see cref="CodeElements"/></param>
        /// <returns>Created parameter's info</returns>
        internal static ParameterTypeInfo[] CreateParametersInfo(CodeElements parameters)
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

        /// <summary>
        /// Get source code from given element
        /// </summary>
        /// <param name="element">Element which source code is retrieved</param>
        /// <returns>Element's source code</returns>
        internal static string GetSourceCode(CodeFunction element)
        {
            if (!element.ProjectItem.IsOpen) element.ProjectItem.Open();
            var editPoint = element.GetStartPoint(vsCMPart.vsCMPartBody).CreateEditPoint();
            var body = editPoint.GetText(element.EndPoint).Replace("\r", "");

            return "{" + body;
        }

        /// <summary>
        /// Create <see cref="TypeDescriptor"/> from given element
        /// </summary>
        /// <param name="element">Element definition of type</param>
        /// <returns>Created <see cref="TypeDescriptor"/></returns>
        internal static TypeDescriptor CreateDescriptor(CodeElement element)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create <see cref="TypeDescriptor"/> from given typeReference
        /// </summary>
        /// <param name="typeRefeference">Reference on type</param>
        /// <returns>Created <see cref="TypeDescriptor"/></returns>
        internal static TypeDescriptor CreateDescriptor(CodeTypeRef typeReference)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates <see cref="TypeDescriptor"/> from given typeNode        
        /// </summary>
        /// <param name="typeNode">Type node which descriptor is created</param>
        /// <returns>Created <see cref="TypeDescriptor"/></returns>
        internal static TypeDescriptor CreateDescriptor(CodeClass typeNode)
        {
            var fullname = typeNode.FullName;

            var typeName = ConvertToTypeName(fullname);

            var descriptor = TypeDescriptor.Create(typeName);
            return descriptor;
        }

        /// <summary>
        /// Converts fullnames between <see cref="CodeModel"/> representation and TypeSystem typeName
        /// </summary>
        /// <param name="fullname">Fullname of element from <see cref="CodeModel"/></param>
        /// <returns></returns>
        internal static string ConvertToTypeName(string fullname)
        {
            throw new NotImplementedException("TODO check fullname form especialy for generics");
        }


        #endregion

        #region Building utilities

        /// <summary>
        /// Set result of build process
        /// </summary>
        /// <param name="resultMethod">Result of building process</param>
        private void Result(MethodItem resultMethod)
        {
            if (_result != null)
                throw new NotSupportedException("Cannot build MethodItem from multiple method definitions");

            _result = resultMethod;
        }

        #endregion

        #region Visitor overrides

        /// <inheritdoc />
        public override void VisitFunction(CodeFunction2 e)
        {
            Result(BuildFrom(e, _declaringAssembly));
        }

        /// <inheritdoc />
        private static void throwNotSupportedElement(CodeElement element)
        {
            throw new NotSupportedException("Given element of type '" + element.GetType() + "' is not supported to be used as method definition");
        }

        #endregion
    }
}
