using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnvDTE;
using EnvDTE80;

using MEFEditor.Analyzing;
using MEFEditor.TypeSystem;
using MEFEditor.Interoperability;

using RecommendedExtensions.Core.AssemblyProviders.ProjectAssembly.Traversing;

namespace RecommendedExtensions.Core.AssemblyProviders.ProjectAssembly.MethodBuilding
{
    /// <summary>
    /// Builder of <see cref="MethodItem"/> objects from <see cref="CodeElement"/> definitions.
    /// </summary>
    public class MethodItemBuilder : CodeElementVisitor
    {
        /// <summary>
        /// Stores result of method build process
        /// </summary>
        private MethodItem _result;

        /// <summary>
        /// Method info of method that will be built
        /// </summary>
        protected TypeMethodInfo MethodInfo { get; private set; }

        /// <summary>
        /// Assembly where built method has been declared
        /// </summary>
        protected VsProjectAssembly DeclaringAssembly { get; private set; }

        /// <summary>
        /// Initialize new instance of <see cref="MethodItem"/> builder.
        /// </summary>        
        public MethodItemBuilder()
        {
            RecursiveVisit = false;
        }

        /// <summary>
        /// Initialize builder with given assembly
        /// </summary>
        /// <param name="declaringAssembly">Assembly where are declared definitions to built</param>
        internal void Initialize(VsProjectAssembly declaringAssembly)
        {
            if (declaringAssembly == null)
                throw new ArgumentNullException("declaringAssembly");

            DeclaringAssembly = declaringAssembly;
        }

        #region Method building API

        /// <summary>
        /// Build <see cref="MethodItem"/> from given element.
        /// </summary>
        /// <param name="element">Method definition element</param>
        /// <param name="needGetter">Determine that getter is needed from given element</param>
        /// <returns>Built method</returns>
        public MethodItem Build(CodeElement element, string requiredName)
        {
            MethodItem result;
            try
            {
                //initialize builder
                _result = null;
                MethodInfo = DeclaringAssembly.InfoBuilder.Build(element, requiredName);
                if (MethodInfo == null)
                    //method cannot be built
                    return null;

                VisitElement(element);
            }
            finally
            {
                MethodInfo = null;

                //reset result
                result = _result;
                _result = null;
            }

            return result;
        }

        /// <summary>
        /// Set result of build process
        /// </summary>
        /// <param name="resultMethod">Result of building process</param>
        protected void Result(MethodItem resultMethod)
        {
            if (_result != null)
                throw new NotSupportedException("Cannot build MethodItem from multiple method definitions");

            _result = resultMethod;
        }

        #endregion

        #region Concrete method builders

        /// <summary>
        /// Build <see cref="MethodItem"/> from given <see cref="CodeFunction"/>.
        /// </summary>
        /// <param name="element">Method definition element</param>
        /// <param name="declaringAssembly">Assembly in which scope method is builded</param>
        /// <returns>Built method</returns>
        public MethodItem BuildFrom(CodeFunction element)
        {
            var sourceCode = element.MustImplement ? null : GetSourceCode(element);
            var isCtor = MethodInfo.MethodName == Naming.CtorName;
            if (isCtor)
            {
                //ctor can have precode
                sourceCode = GetPreCode(element) + sourceCode;
            }

            var namespaces = DeclaringAssembly.GetNamespaces(element as CodeElement);

            //get generic parameters info
            var fullname = element.FullName;
            var genericPath = new PathInfo(fullname);

            var activation = new ParsingActivation(sourceCode, MethodInfo, genericPath.GenericArgs, namespaces);
            RegisterActivation(activation, element as CodeElement);

            var generator = new SourceMethodGenerator(activation, DeclaringAssembly.ParsingProvider);
            var item = new MethodItem(generator, MethodInfo);
            return item;
        }

        /// <summary>
        /// Build <see cref="MethodItem"/> from given variable element
        /// </summary>
        /// <param name="element">Method definition element</param>
        /// <param name="buildGetter">Determine that getter or setter should be builded</param>
        /// <returns>Built method</returns>
        public MethodItem BuildFrom(CodeVariable element)
        {
            //variable will generate auto property
            return buildAutoProperty(MethodInfo);
        }


        /// <summary>
        /// Build <see cref="MethodItem"/> from given <see cref="CodeProperty"/> element
        /// </summary>
        /// <param name="element">Method definition element</param>
        /// <param name="buildGetter">Determine that getter or setter should be builded</param>
        /// <returns>Built method</returns>
        public MethodItem BuildFrom(CodeProperty element)
        {
            var isAutoProperty = element.IsAutoProperty();
            if (isAutoProperty || MethodInfo.IsAbstract)
                return buildAutoProperty(MethodInfo);

            var buildGetter = MethodInfo.MethodName.StartsWith(Naming.GetterPrefix);
            var method = buildGetter ? element.Getter : element.Setter;

            if (method == null)
                return null;

            return BuildFrom(method);
        }

        #endregion

        #region Building utilities

        /// <summary>
        /// Get source code from given element
        /// </summary>
        /// <param name="element">Element which source code is retrieved</param>
        /// <returns>Element's source code</returns>
        internal string GetSourceCode(CodeFunction element)
        {
            var name = element.Name;
            var lang = element.Language;
            if (!element.ProjectItem.IsOpen) element.ProjectItem.Open();
            var editPoint = element.GetStartPoint(vsCMPart.vsCMPartBody).CreateEditPoint();
            var body = editPoint.GetText(element.EndPoint).Replace("\r", "");

            return "{" + body;
        }

        /// <summary>
        /// Get source code from given element that is before main body
        /// </summary>
        /// <param name="fn">Element which precode is retrieved</param>
        /// <returns>Element's precode</returns>
        internal string GetPreCode(CodeFunction fn)
        {
            if (fn.FunctionKind != vsCMFunction.vsCMFunctionConstructor)
            {
                //precode is available only for ctors
                return "";
            }

            var name = fn.Name;
            var lang = fn.Language;
            if (!fn.ProjectItem.IsOpen) fn.ProjectItem.Open();
            var editPoint = fn.GetStartPoint(vsCMPart.vsCMPartHeader).CreateEditPoint();
            var endPoint = fn.GetStartPoint(vsCMPart.vsCMPartBody);
            var preCode = editPoint.GetText(endPoint).Replace("\r", "");
            var preCodeStart = preCode.IndexOf(':');

            if (preCodeStart < 0)
                return "";

            preCode = preCode.Substring(preCodeStart);
            var bodyStart = preCode.LastIndexOf('{');
            preCode = preCode.Substring(0, bodyStart).Trim();

            return preCode + (char)0;
        }

        /// <summary>
        /// Build auto generated property from given <see cref="TypeMethodInfo"/>
        /// </summary>
        /// <param name="methodInfo">Info of method that will be generated</param>
        /// <returns></returns>
        private static MethodItem buildAutoProperty(TypeMethodInfo methodInfo)
        {
            var buildGetter = !methodInfo.ReturnType.Equals(TypeDescriptor.Void);
            var methodName = methodInfo.MethodName;
            var propertyStorage = buildGetter ? methodName.Substring(Naming.GetterPrefix.Length) : methodName.Substring(Naming.SetterPrefix.Length);
            propertyStorage = "@" + propertyStorage;

            if (buildGetter)
            {
                var getterGenerator = new DirectGenerator((c) =>
                {
                    var fieldValue = c.GetField(c.CurrentArguments[0], propertyStorage) as Instance;
                    c.Return(fieldValue);
                });

                return new MethodItem(getterGenerator, methodInfo);
            }
            else
            {
                var setterGenerator = new DirectGenerator((c) =>
                {
                    var setValue = c.CurrentArguments[1];
                    c.SetField(c.CurrentArguments[0], propertyStorage, setValue);
                });

                return new MethodItem(setterGenerator, methodInfo);
            }
        }

        #endregion

        #region Changes writing services

        /// <summary>
        /// Register handlers on activation for purposes of binding with source
        /// </summary>
        /// <param name="activation">Activation which bindings will be registered</param>
        /// <param name="element">Element where bindings will be applied</param>
        protected void RegisterActivation(ParsingActivation activation, CodeElement element)
        {
            var fn = element as CodeFunction;
            if (fn != null)
                activation.SourceChangeCommited += (source, ns) => { Write(fn, source, ns); };
            activation.NavigationRequested += (offset) => Navigate(element, offset);
        }

        /// <summary>
        /// Process source writing into given element
        /// </summary>
        /// <param name="element">Element which source will be written</param>
        /// <param name="requiredNamespaces">Namespaces which presence is required within document</param>
        /// <param name="source">Source that will be written</param>
        protected void Write(CodeFunction element, string source, IEnumerable<string> requiredNamespaces)
        {
            bool undoOpened = false;

            var dte = element.DTE;
            if (dte != null && !dte.UndoContext.IsOpen)
            {
                dte.UndoContext.Open("MEF Component Architecture Editor Change");
                undoOpened = true;
            }

            try
            {
                var editPoint = getEditPoint(element);
                if (element.ProjectItem.Document.ReadOnly)
                    throw new NotSupportedException("Document is read only");

                var fileCodeModel2 = element.ProjectItem.FileCodeModel as FileCodeModel2;
                if (fileCodeModel2 != null)
                {
                    foreach (var ns in requiredNamespaces)
                    {
                        fileCodeModel2.AddImport(ns);
                    }
                }

                source = source.Substring(1); //remove first {
                editPoint.ReplaceText(element.EndPoint, source, (int)vsEPReplaceTextOptions.vsEPReplaceTextAutoformat);
            }
            finally
            {
                if (undoOpened)
                    dte.UndoContext.Close();
            }
        }

        /// <summary>
        /// Process navigating at offset on given element
        /// </summary>
        /// <param name="element">Element which is base for offset navigation</param>
        /// <param name="navigationOffset">Offset where user will be navigated to</param>
        protected void Navigate(CodeElement element, int navigationOffset)
        {
            if (!(element is CodeFunction))
                navigationOffset = 0;

            element.ProjectItem.Open();
            var doc = element.ProjectItem.Document;
            if (doc == null)
                //document is unavailable
                return;
            //activate document to get it visible to user.
            doc.Activate();

            //part of CodeElement where navigation offset start.
            vsCMPart part;
            if (element.Kind == vsCMElement.vsCMElementFunction)
                //functions are navigated into body
                part = vsCMPart.vsCMPartBody;
            else
                //else navigate from element begining
                part = vsCMPart.vsCMPartWholeWithAttributes;

            TextPoint start;
            try
            {
                start = element.GetStartPoint(part);
            }
            catch (Exception)
            {
                //cannot navigate
                return;
            }

            var sel = doc.Selection as TextSelection;
            //Shift cursor at navigation position.
            sel.MoveToAbsoluteOffset(start.AbsoluteCharOffset + navigationOffset - 1);
        }

        /// <summary>
        /// Create edit point which can be used for writing into element body.
        /// </summary>
        /// <param name="element">Element to get edit point from.</param>
        /// <returns>Created edit point.</returns>
        private EditPoint getEditPoint(CodeFunction element)
        {
            return element.GetStartPoint(vsCMPart.vsCMPartBody).CreateEditPoint();
        }

        #endregion

        #region Visitor overrides

        /// <inheritdoc />
        public override void VisitFunction(CodeFunction2 e)
        {
            Result(BuildFrom(e));
        }

        /// <inheritdoc />
        public override void VisitVariable(CodeVariable e)
        {
            Result(BuildFrom(e));
        }

        /// <inheritdoc />
        public override void VisitProperty(CodeProperty e)
        {
            Result(BuildFrom(e));
        }

        #endregion

    }
}
