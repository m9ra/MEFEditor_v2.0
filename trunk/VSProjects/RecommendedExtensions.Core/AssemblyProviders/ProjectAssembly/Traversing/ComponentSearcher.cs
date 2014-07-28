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
using RecommendedExtensions.Core.AssemblyProviders.ProjectAssembly.MethodBuilding;

namespace RecommendedExtensions.Core.AssemblyProviders.ProjectAssembly.Traversing
{
    /// <summary>
    /// Searcher provides descending searching of components from visited
    /// CodeElement.
    /// </summary>
    class ComponentSearcher : CodeElementVisitor
    {
        /// <summary>
        /// Event fired whenever new component is found
        /// </summary>
        internal event ComponentEvent OnComponentFound;

        /// <summary>
        /// <see cref="TypeServices"/> used for resolving types' inheritance
        /// </summary>
        private readonly TypeServices _services;

        /// <summary>
        /// Assembly using current searcher
        /// </summary>
        private readonly VsProjectAssembly _assembly;


        /// <summary>
        /// Indexes of built components
        /// </summary>
        private Dictionary<CodeClass, ComponentInfoBuilder> _builtComponents = new Dictionary<CodeClass, ComponentInfoBuilder>();

        /// <summary>
        /// Initialize instance of <see cref="ComponentSearcher"/>
        /// </summary>
        /// <param name="services"><see cref="TypeServices"/> used for resolving types' inheritance</param>
        /// <param name="assembly">Assembly using current searcher</param>
        internal ComponentSearcher(VsProjectAssembly assembly, TypeServices services)
        {
            if (services == null)
                throw new ArgumentNullException("services");

            if (assembly == null)
                throw new ArgumentNullException("assembly");

            _services = services;
            _assembly = assembly;
        }

        #region Visitor overrides

        /// <inheritdoc />
        public override void VisitClass(CodeClass2 e)
        {
            base.VisitClass(e);

            if (_builtComponents.ContainsKey(e))
            {
                //component has been found
                var componentBuilder = _builtComponents[e];

                if (!componentBuilder.HasImportingCtor)
                {
                    if (hasParamLessConstructor(e))
                        componentBuilder.AddImplicitImportingConstructor();
                }

                //check componet's implicit composition point
                if (!componentBuilder.HasCompositionPoint)
                {
                    if (hasParamLessConstructor(e))
                        componentBuilder.AddImplicitCompositionPoint();
                }

                var componentInfo = componentBuilder.Build();
                OnComponentFound(componentInfo);
            }
        }

        /// <inheritdoc />
        public override void VisitAttribute(CodeAttribute2 e)
        {
            var fullname = e.SafeFullname();

            if (
                fullname == Naming.ExportAttribute ||
                fullname == Naming.InheritedExportAttribute
                )
            {
                addExport(new AttributeInfo(e));
            }
            else if (fullname == Naming.ImportAttribute)
            {
                addImport(new AttributeInfo(e));
            }
            else if (fullname == Naming.ImportManyAttribute)
            {
                addImport(new AttributeInfo(e), true);
            }
            else if (fullname == Naming.CompositionPointAttribute)
            {
                addCompositionPoint(new AttributeInfo(e));
            }
            else if (fullname == Naming.ImportingConstructorAttribute)
            {
                addImportingConstructor(new AttributeInfo(e));
            }
        }

        #endregion

        #region Component building helpers

        /// <summary>
        /// Add import according to given <see cref="CodeAttribute"/>
        /// </summary>
        /// <param name="importAttribute">Attribute defining import</param>
        /// <param name="forceMany">Determine that explicit <c>AllowMany</c> is used</param>
        private void addImport(AttributeInfo importAttribute, bool forceMany = false)
        {
            var builder = getOrCreateCurrentBuilder(importAttribute.Element as CodeElement);

            MethodID importMethodID;
            TypeDescriptor importType;
            if (!getImportTarget(importAttribute.Element, builder.ComponentType, out importMethodID, out importType))
            {
                _assembly.VS.Log.Warning("Cannot parse import attribute on: " + importAttribute.Element.Name);
                return;
            }

            var explicitContract = parseContract(importAttribute.GetArgument(0), builder, importAttribute.Element);

            var allowMany = forceMany || importAttribute.IsTrue("AllowMany");
            var allowDefault = forceMany || importAttribute.IsTrue("AllowDefault");

            var importTypeInfo = ImportTypeInfo.ParseFromMany(importType, allowMany, _services);
            var contract = explicitContract == null ? importTypeInfo.ItemType.TypeName : explicitContract;

            builder.AddImport(importTypeInfo, importMethodID, contract, allowMany, allowDefault);
        }

        /// <summary>
        /// Add export according to given <see cref="CodeAttribute"/>
        /// </summary>
        /// <param name="exportAttribute">Attribute defining export</param>
        private void addExport(AttributeInfo exportAttribute)
        {
            var builder = getOrCreateCurrentBuilder(exportAttribute.Element as CodeElement);

            TypeDescriptor exportTypeDescriptor;
            MethodID exportMethodID;
            if (!getExportTarget(exportAttribute.Element, builder.ComponentType, out exportMethodID, out exportTypeDescriptor))
            {
                _assembly.VS.Log.Warning("Cannot parse export attribute on: " + exportAttribute.Element.Name);
                return;
            }

            var explicitContract = parseContract(exportAttribute.GetArgument(0), builder, exportAttribute.Element);
            var contract = explicitContract == null ? exportTypeDescriptor.TypeName : explicitContract;
            var isSelfExport = exportMethodID == null;
            var isInherited = exportAttribute.Element.FullName == Naming.InheritedExportAttribute;

            exploreMetaData(exportAttribute, builder);

            if (isSelfExport)
            {
                builder.AddSelfExport(isInherited, contract);
            }
            else
            {
                builder.AddExport(exportTypeDescriptor, exportMethodID, isInherited, contract);
            }
        }

        private void exploreMetaData(AttributeInfo exportAttribute, ComponentInfoBuilder builder)
        {
            var target = exportAttribute.Element.Parent as CodeElement;
            if (target == null)
                return;

            //check target attributes whether metadata is exported
            var attributes = target.GetAttributes();
            foreach (CodeAttribute2 attribute in attributes)
            {
                if (attribute.FullName == Naming.ExportMetadataAttribute)
                {
                    var info = new AttributeInfo(attribute);
                    buildMetaExport(info, builder);
                }
            }

        }

        private void buildMetaExport(AttributeInfo info, ComponentInfoBuilder builder)
        {
            var isMultiple = info.GetArgument("IsMultiple") == "true";
            var name = parseObject(info.GetArgument("Name", 0), builder, info.Element) as string;
            var value = parseObject(info.GetArgument("Value", 1), builder, info.Element);

            builder.AddMeta(name, value, isMultiple);
        }

        /// <summary>
        /// Get export target description where given attribute is defined
        /// </summary>
        /// <param name="attribute">Export attribute</param>
        /// <param name="componentType">Type of defining component</param>
        /// <param name="exportMethodID">Id of method that can be used for export. It is <c>null</c> for self exports</param>
        /// <param name="exportType">Type of defined export</param>
        /// <returns><c>true</c> if target has been successfully found, <c>false</c> otherwise</returns>
        private bool getExportTarget(CodeAttribute2 attribute, TypeDescriptor componentType, out MethodID exportMethodID, out TypeDescriptor exportType)
        {
            var target = attribute.Parent as CodeElement;

            exportMethodID = null;
            exportType = null;

            var name = target.Name();

            switch (target.Kind)
            {
                case vsCMElement.vsCMElementVariable:
                    //variables are represented by properties within type system
                    exportMethodID = Naming.Method(componentType, Naming.GetterPrefix + name, false, ParameterTypeInfo.NoParams);
                    exportType = _assembly.InfoBuilder.CreateDescriptor((target as CodeVariable).Type);
                    return true;

                case vsCMElement.vsCMElementProperty:
                    exportMethodID = Naming.Method(componentType, Naming.GetterPrefix + name, false, ParameterTypeInfo.NoParams);
                    exportType = _assembly.InfoBuilder.CreateDescriptor((target as CodeProperty).Type);
                    return true;

                case vsCMElement.vsCMElementClass:
                    //self export doesnt need exportMethodID
                    exportType = _assembly.InfoBuilder.CreateDescriptor(target as CodeClass);
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Get import target description where given attribute is defined
        /// </summary>
        /// <param name="attribute">Import attribute</param>
        /// <param name="componentType">Type of defining component</param>
        /// <param name="importMethodID">Id of method that can be used for import. It is <c>null</c> for self exports</param>
        /// <param name="importType">Type of defined export</param>
        /// <returns><c>true</c> if target has been successfully found, <c>false</c> otherwise</returns>
        private bool getImportTarget(CodeAttribute2 attribute, TypeDescriptor componentType, out MethodID importMethodID, out TypeDescriptor importType)
        {
            var target = attribute.Parent as CodeElement;

            importMethodID = null;
            importType = null;

            var name = target.Name();

            switch (target.Kind)
            {
                case vsCMElement.vsCMElementVariable:
                    //variables are represented by properties within type system
                    importType = _assembly.InfoBuilder.CreateDescriptor((target as CodeVariable).Type);
                    importMethodID = Naming.Method(componentType, Naming.SetterPrefix + name, false,
                        ParameterTypeInfo.Create("value", importType)
                        );
                    return true;

                case vsCMElement.vsCMElementProperty:
                    importType = _assembly.InfoBuilder.CreateDescriptor((target as CodeProperty).Type);
                    importMethodID = Naming.Method(componentType, Naming.SetterPrefix + name, false,
                        ParameterTypeInfo.Create("value", importType)
                        );
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Add importing constructor according to given <see cref="CodeAttribute"/>
        /// </summary>
        /// <param name="compositionAttrbiute">Attribute defining importing constructor</param>
        private void addImportingConstructor(AttributeInfo compositionAttrbiute)
        {
            var methodElement = getMethod(compositionAttrbiute.Element) as CodeElement;
            if (methodElement == null)
            {
                _assembly.VS.Log.Warning("Method marked with importing constructor cannot be loaded");
            }

            var info = _assembly.InfoBuilder.Build(methodElement, Naming.CtorName);

            var builder = getOrCreateCurrentBuilder(compositionAttrbiute.Element as CodeElement);
            builder.SetImportingCtor(info);
        }

        /// <summary>
        /// Add CompositionPoint according to given <see cref="CodeAttribute"/>
        /// </summary>
        /// <param name="compositionAttribute">Attribute defining export</param>
        private void addCompositionPoint(AttributeInfo compositionAttribute)
        {
            var method = getMethod(compositionAttribute.Element);
            var methodElement = method as CodeElement;
            if (methodElement == null)
            {
                _assembly.VS.Log.Warning("Method marked with composition point cannot be loaded");
            }

            var isCtor = method.FunctionKind == vsCMFunction.vsCMFunctionConstructor;
            var isStatic = method.IsShared;
            var ctorName = isStatic ? Naming.ClassCtorName : Naming.CtorName;
            var name = isCtor ? ctorName : methodElement.Name;

            var info = _assembly.InfoBuilder.Build(methodElement, name);

            var builder = getOrCreateCurrentBuilder(compositionAttribute.Element as CodeElement);
            builder.AddExplicitCompositionPoint(info.MethodID, createInitializer(compositionAttribute, info));
        }

        #region Literal parsing

        private string parseContract(string rawContract, ComponentInfoBuilder builder, CodeAttribute2 attribute)
        {
            if (rawContract == null)
                return null;

            var parsed = parseObject(rawContract, builder, attribute);
            var type = parsed as InstanceInfo;
            if (type != null)
                return type.TypeName;

            return parsed as string;
        }



        private object parseObject(string data, ComponentInfoBuilder builder, CodeAttribute2 attribute)
        {
            return _assembly.ParseValue(data, builder.ComponentType, attribute as CodeElement);
        }

        #endregion

        private GeneratorBase createInitializer(AttributeInfo compositionAttribute, TypeMethodInfo compositionPointInfo)
        {
            if (compositionPointInfo.Parameters.Length == 0)
                //no arguments are required
                return null;

            if (compositionPointInfo.Parameters.Length != compositionAttribute.PositionalArgumentsCount)
                _assembly.VS.Log.Error("Detected explicit composition point with wrong argument count for {0}", compositionPointInfo.MethodID);

            return new InitializerGenerator(_assembly, compositionAttribute, compositionPointInfo);
        }

        /// <summary>
        /// Get <see cref="CodeProperty"/> where given attribute is defined
        /// </summary>
        /// <param name="attribute">Attribute which property is needed</param>
        /// <returns><see cref="CodeProperty"/> where given attribute is defined, <c>null</c> if there is no such property</returns>
        private CodeProperty getProperty(CodeAttribute attribute)
        {
            return attribute.Parent as CodeProperty;
        }

        /// <summary>
        /// Get <see cref="CodeVariable"/> where given attribute is defined
        /// </summary>
        /// <param name="attribute">Attribute which property is needed</param>
        /// <returns><see cref="CodeVariable"/> where given attribute is defined, <c>null</c> if there is no such variable</returns>
        private CodeVariable getProperty(CodeVariable attribute)
        {
            return attribute.Parent as CodeVariable;
        }

        /// <summary>
        /// Get <see cref="CodeProperty"/> where given attribute is defined
        /// </summary>
        /// <param name="attribute">Attribute which property is needed</param>
        /// <returns><see cref="CodeProperty"/> where given attribute is defined, <c>null</c> if there is no such property</returns>
        private CodeFunction getMethod(CodeAttribute attribute)
        {
            return attribute.Parent as CodeFunction;
        }

        /// <summary>
        /// Get or create <see cref="ComponentInfoBuilder"/> for class owning currently visited element
        /// </summary>
        /// <returns><see cref="ComponentInfoBuilder"/> for currently visited class</returns>
        private ComponentInfoBuilder getOrCreateCurrentBuilder(CodeElement element)
        {
            var currentClass = element.DeclaringClass();

            ComponentInfoBuilder builder;
            if (!_builtComponents.TryGetValue(currentClass, out builder))
            {
                _builtComponents[currentClass] = builder = new ComponentInfoBuilder(_assembly.InfoBuilder.CreateDescriptor(currentClass));
            }

            return builder;
        }

        /// <summary>
        /// Determine that given class has parameter less constructor (implicit or not)
        /// </summary>
        /// <param name="element">Class element to be tested</param>
        /// <returns><c>true</c> if class has parameter less constructor, <c>false</c> otherwise</returns>
        private bool hasParamLessConstructor(CodeClass2 element)
        {
            var hasImplicitParamLessCtor = true;

            foreach (var member in element.Members)
            {
                var function = member as CodeFunction;
                if (function == null)
                    continue;

                if (function.FunctionKind == vsCMFunction.vsCMFunctionConstructor)
                {
                    //there already exist constructor which prohibits implicit one
                    hasImplicitParamLessCtor = false;

                    if (function.Parameters.Count == 0)
                        //param less ctor exist - implicit composition point is possible
                        return true;
                }
            }

            //no constructor that prohibits implicit compositoin point exist
            return hasImplicitParamLessCtor;
        }

        #endregion
    }
}
