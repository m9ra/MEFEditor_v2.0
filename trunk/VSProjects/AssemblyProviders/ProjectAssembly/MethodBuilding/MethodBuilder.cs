﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnvDTE;
using EnvDTE80;

using Analyzing;
using TypeSystem;
using Interoperability;

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
        /// Determine that getter is need
        /// </summary>
        private bool _needGetter;

        /// <summary>
        /// Assembly where builded method has been declared
        /// </summary>
        private readonly VsProjectAssembly _declaringAssembly;

        /// <summary>
        /// Hide constructor - Static Build method should be used
        /// </summary>
        private MethodBuilder(bool needGetter, VsProjectAssembly declaringAssembly)
        {
            if (declaringAssembly == null)
                throw new ArgumentNullException("declaringAssembly");

            _needGetter = needGetter;
            _declaringAssembly = declaringAssembly;
        }

        #region Method building API

        /// <summary>
        /// Build <see cref="MethodItem"/> from given element.
        /// </summary>
        /// <param name="element">Method definition element</param>
        /// <param name="declaringAssembly">Assembly where builded method has been declared</param>
        /// <param name="needGetter">Determine that getter is needed from given element</param>
        /// <returns>Builded method</returns>
        internal static MethodItem Build(CodeElement element, bool needGetter, VsProjectAssembly declaringAssembly)
        {
            var builder = new MethodBuilder(needGetter, declaringAssembly);

            builder.VisitElement(element);

            var result = builder._result;

            if (result == null)
                throwNotSupportedElement(element);

            return result;
        }

        #region Concrete method builders

        /// <summary>
        /// Build <see cref="MethodItem"/> from given <see cref="CodeFunction"/>.
        /// </summary>
        /// <param name="element">Method definition element</param>
        /// <param name="declaringAssembly">Assembly in which scope method is builded</param>
        /// <returns>Builded method</returns>
        internal static MethodItem BuildFrom(CodeFunction element, VsProjectAssembly declaringAssembly)
        {
            var methodInfo = CreateMethodInfo(element);

            return BuildFrom(element, methodInfo, declaringAssembly);
        }


        /// <summary>
        /// Build <see cref="MethodItem"/> from given <see cref="CodeFunction"/>.
        /// </summary>
        /// <param name="element">Method definition element</param>
        /// <param name="declaringAssembly">Assembly in which scope method is builded</param>
        /// <returns>Builded method</returns>
        private static MethodItem BuildFrom(CodeFunction element, TypeMethodInfo methodInfo, VsProjectAssembly declaringAssembly)
        {
            var sourceCode = GetSourceCode(element);
            var namespaces = declaringAssembly.GetNamespaces(element);

            var fullname = element.FullName;
            var genericPath = new PathInfo(fullname);
            var activation = new ParsingActivation(sourceCode, methodInfo, genericPath.GenericArgs, namespaces);
            registerCommits(activation, element);

            var generator = new SourceMethodGenerator(activation, declaringAssembly.ParsingProvider);

            var item = new MethodItem(generator, methodInfo);
            return item;
        }

        /// <summary>
        /// Build <see cref="MethodItem"/> from given variable element
        /// </summary>
        /// <param name="element">Method definition element</param>
        /// <param name="buildGetter">Determine that getter or setter should be builded</param>
        /// <returns>Builded method</returns>
        internal static MethodItem BuildFrom(CodeVariable element, bool buildGetter)
        {
            var methodInfo = CreateMethodInfo(element, buildGetter);

            //variable will generate auto property
            return buildAutoProperty(methodInfo);
        }

        /// <summary>
        /// Build <see cref="MethodItem"/> from given <see cref="CodeProperty"/> element
        /// </summary>
        /// <param name="element">Method definition element</param>
        /// <param name="buildGetter">Determine that getter or setter should be builded</param>
        /// <returns>Builded method</returns>
        internal static MethodItem BuildFrom(CodeProperty element, bool buildGetter, VsProjectAssembly declaringAssembly)
        {
            var isAutoProperty = element.IsAutoProperty();
            var methodInfo = CreateMethodInfo(element, buildGetter);

            if (isAutoProperty)
                return buildAutoProperty(methodInfo);

            var method = buildGetter ? element.Getter : element.Setter;

            return BuildFrom(method, methodInfo, declaringAssembly);
        }

        #endregion

        /// <summary>
        /// Creates <see cref="TypeMethodInfo"/> for given element
        /// </summary>
        /// <param name="element">Element which <see cref="TypeMethodInfo"/> is created</param>
        /// <returns>Created <see cref="TypeMethodInfo"/></returns>
        internal static TypeMethodInfo CreateMethodInfo(CodeFunction element)
        {
            //TODO check if parent is property -> different naming conventions

            //collect information from element - note, every call working with element may fail with exception, because of VS doesn't provide determinism
            var name = GetName(element);
            var isShared = element.IsShared;
            var isAbstract = element.MustImplement;
            var declaringType = CreateDescriptor(element.DeclaringClass());
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
        /// Get name representation of given <see cref="CodeFunction"/>
        /// </summary>
        /// <param name="element">Element representing function</param>
        /// <returns>Function name representation</returns>
        internal static string GetName(CodeFunction element)
        {
            var name = element.Name;
            var isConstructor = element.FunctionKind == vsCMFunction.vsCMFunctionConstructor;
            var isShared = element.IsShared;

            if (isConstructor)
            {
                //repair name according to naming conventions
                name = isShared ? Naming.ClassCtorName : Naming.CtorName;
            }

            return name;
        }

        /// <summary>
        /// Get fullname representation of given <see cref="CodeFunction"/>
        /// </summary>
        /// <param name="element">Element representing function</param>
        /// <returns>Function name representation</returns>
        internal static string GetFullName(CodeFunction element)
        {
            //TODO correctnes
            var name = GetName(element);
            var parentFullname = element.DeclaringClass().FullName;

            return parentFullname + "." + name;
        }


        /// <summary>
        /// Get fullname representation of given <see cref="CodeFunction"/>
        /// </summary>
        /// <param name="element">Element representing function</param>
        /// <returns>Function name representation</returns>
        internal static string GetFullName(CodeElement element)
        {
            switch (element.Kind)
            {
                case vsCMElement.vsCMElementFunction:
                    return GetFullName(element as CodeFunction);
                default:
                    //TODO correctnes
                    return element.FullName;
            }
        }

        /// <summary>
        /// Creates <see cref="TypeMethodInfo"/> for given element
        /// </summary>
        /// <param name="element">Element which <see cref="TypeMethodInfo"/> is created</param>
        /// <param name="buildGetter"></param>
        /// <returns>Created <see cref="TypeMethodInfo"/></returns>
        internal static TypeMethodInfo CreateMethodInfo(CodeProperty element, bool buildGetter)
        {
            var namePrefix = buildGetter ? Naming.GetterPrefix : Naming.SetterPrefix;
            var name = namePrefix + element.Name;

            var method = buildGetter ? element.Getter : element.Setter;
            var property2 = element as CodeProperty2;

            var isShared = property2 != null && property2.IsShared;
            var isAbstract = method.MustImplement;
            var declaringType = CreateDescriptor(element.Parent as CodeClass);
            var variableType = CreateDescriptor(element.Type);

            //variables cannot have type arguments
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

            var methodInfo = new TypeMethodInfo(
                declaringType, name, returnType, parameters,
                isShared, methodTypeArguments, isAbstract
                );

            return methodInfo;
        }

        /// <summary>
        /// Creates <see cref="TypeMethodInfo"/> for given element
        /// </summary>
        /// <param name="element">Element which <see cref="TypeMethodInfo"/> is created</param>
        /// <param name="buildGetter"></param>
        /// <returns>Created <see cref="TypeMethodInfo"/></returns>
        internal static TypeMethodInfo CreateMethodInfo(CodeVariable element, bool buildGetter)
        {
            var namePrefix = buildGetter ? Naming.GetterPrefix : Naming.SetterPrefix;
            var name = namePrefix + element.Name;

            var isShared = element.IsShared;
            var isAbstract = false; //variables cannot be abstract
            var declaringType = CreateDescriptor(element.Parent as CodeClass);
            var variableType = CreateDescriptor(element.Type);

            //variables cannot have type arguments
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
            var name = element.Name;
            var lang = element.Language;
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
            if (element is CodeTypeRef)
                return CreateDescriptor(element as CodeTypeRef);

            if (element is CodeClass)
                return CreateDescriptor(element as CodeClass);

            var name = element.FullName;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create <see cref="TypeDescriptor"/> from given typeReference
        /// </summary>
        /// <param name="typeRefeference">Reference on type</param>
        /// <returns>Created <see cref="TypeDescriptor"/></returns>
        internal static TypeDescriptor CreateDescriptor(CodeTypeRef typeReference)
        {
            var fullname = typeReference.AsFullName;
            if (fullname == "")
                return TypeDescriptor.Void;

            return ConvertToDescriptor(fullname);
        }

        /// <summary>
        /// Creates <see cref="TypeDescriptor"/> from given typeNode        
        /// </summary>
        /// <param name="typeNode">Type node which descriptor is created</param>
        /// <returns>Created <see cref="TypeDescriptor"/></returns>
        internal static TypeDescriptor CreateDescriptor(CodeClass typeNode)
        {
            var fullname = typeNode.FullName;

            return ConvertToDescriptor(fullname);
        }

        /// <summary>
        /// Converts fullnames between <see cref="CodeModel"/> representation and TypeSystem typeName
        /// </summary>
        /// <param name="fullname">Fullname of element from <see cref="CodeModel"/></param>
        /// <returns></returns>
        internal static TypeDescriptor ConvertToDescriptor(string fullname)
        {
            // throw new NotImplementedException("TODO check fullname form especialy for generics");
            var descriptor = TypeDescriptor.Create(fullname);

            return descriptor;
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

        /// <summary>
        /// Build auto generated property from given <see cref="TypeMethodInfo"/>
        /// </summary>
        /// <param name="methodInfo">Info of method that will be generated</param>
        /// <returns></returns>
        private static MethodItem buildAutoProperty(TypeMethodInfo methodInfo)
        {
            var buildGetter = !methodInfo.ReturnType.Equals(TypeDescriptor.Void);

            if (buildGetter)
            {
                var getterGenerator = new DirectGenerator((c) =>
                {
                    var fieldValue = c.GetField(c.CurrentArguments[0], methodInfo.MethodName) as Instance;
                    c.Return(fieldValue);
                });

                return new MethodItem(getterGenerator, methodInfo);
            }
            else
            {
                var setterGenerator = new DirectGenerator((c) =>
                {
                    var setValue = c.CurrentArguments[1];
                    c.SetField(c.CurrentArguments[0], methodInfo.MethodName, setValue);
                });

                return new MethodItem(setterGenerator, methodInfo);
            }
        }

        #endregion

        #region Visitor overrides

        /// <inheritdoc />
        public override void VisitFunction(CodeFunction2 e)
        {
            Result(BuildFrom(e, _declaringAssembly));
        }

        /// <inheritdoc />
        public override void VisitVariable(CodeVariable e)
        {
            Result(BuildFrom(e, _needGetter));
        }

        /// <inheritdoc />
        public override void VisitProperty(CodeProperty e)
        {
            Result(BuildFrom(e, _needGetter, _declaringAssembly));
        }

        /// <inheritdoc />
        private static void throwNotSupportedElement(CodeElement element)
        {
            throw new NotSupportedException("Given element of type '" + element.GetType() + "' is not supported to be used as method definition");
        }

        #endregion

        #region Changes writing services

        /// <summary>
        /// Register commits on activation for writing purposes
        /// </summary>
        /// <param name="activation">Activation which commits will be registered</param>
        /// <param name="element">Element where commited source will be written</param>
        private static void registerCommits(ParsingActivation activation, CodeFunction element)
        {
            activation.SourceChangeCommited += (source) => write(element, source);
        }

        /// <summary>
        /// Process source writing into given element
        /// </summary>
        /// <param name="element">Element which source will be written</param>
        /// <param name="source">Source that will be written</param>
        private static void write(CodeFunction element, string source)
        {
            var editPoint = getEditPoint(element);
            if (element.ProjectItem.Document.ReadOnly)
                throw new NotSupportedException("Document is read only");

            //TODO this is ugly - refactor getting source code
            source = source.Substring(1); //remove first {

            editPoint.ReplaceText(element.EndPoint, source, (int)vsEPReplaceTextOptions.vsEPReplaceTextAutoformat);
        }

        /// <summary>
        /// Create edit point which can be used for writing into element body.
        /// </summary>
        /// <param name="element">Element to get edit point from.</param>
        /// <returns>Created edit point.</returns>
        private static EditPoint getEditPoint(CodeFunction element)
        {
            return element.GetStartPoint(vsCMPart.vsCMPartBody).CreateEditPoint();
        }

        #endregion
    }
}
