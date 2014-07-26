using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnvDTE;
using EnvDTE80;

using Analyzing;
using TypeSystem;
using Interoperability;
using AssemblyProviders.ProjectAssembly.MethodBuilding;

namespace AssemblyProviders.ProjectAssembly.Traversing
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

                //check componts implicit composition point
                if (!componentBuilder.HasCompositionPoint)
                {
                    if (couldHaveImplicitCompositionPoint(e))
                        componentBuilder.AddImplicitCompositionPoint();
                }

                var componentInfo = componentBuilder.BuildInfo();
                OnComponentFound(componentInfo);
            }
        }

        /// <inheritdoc />
        public override void VisitAttribute(CodeAttribute2 e)
        {
            //TODO maybe catching exceptions for some search level will be advantageous
            var fullname = e.SafeFullname();

            if (fullname == Naming.ExportAttribute)
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
            else if (fullname == Naming.ImportingConstructor)
            {
                addImportingConstructor(new AttributeInfo(e));
            }
        }

        #endregion

        #region Component building helpers

        /// <summary>
        /// Add import according to given <see cref="CodeAttribute"/>
        /// </summary>
        /// <param name="importAttrbute">Attribute defining import</param>
        /// <param name="forceMany">Determine that explicit <c>AllowMany</c> is used</param>
        private void addImport(AttributeInfo importAttrbute, bool forceMany = false)
        {
            var builder = getOrCreateCurrentBuilder(importAttrbute.Element as CodeElement);

            MethodID importMethodID;
            TypeDescriptor importType;
            if (!getImportTarget(importAttrbute.Element, builder.ComponentType, out importMethodID, out importType))
            {
                //TODO log that import attribute cannot be handled
                return;
            }

            var explicitContract = parseContract(importAttrbute.GetArgument(0), builder, importAttrbute.Element);

            var allowMany = forceMany || importAttrbute.IsTrue("AllowMany");
            var allowDefault = forceMany || importAttrbute.IsTrue("AllowDefault");

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
                //TODO log that export attribute cannot be handled
                return;
            }

            var explicitContract = parseContract(exportAttribute.GetArgument(0), builder, exportAttribute.Element);
            var contract = explicitContract == null ? exportTypeDescriptor.TypeName : explicitContract;
            var isSelfExport = exportMethodID == null;

            exploreMetaData(exportAttribute, builder);

            if (isSelfExport)
            {
                builder.AddSelfExport(contract);
            }
            else
            {
                builder.AddExport(exportTypeDescriptor, exportMethodID, contract);
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
            var name = parseString(info.GetArgument("Name", 0));
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
        /// <returns><c>true</c> if target has been succesfully found, <c>false</c> otherwise</returns>
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
                    exportType = MethodBuilder.CreateDescriptor((target as CodeVariable).Type);
                    return true;

                case vsCMElement.vsCMElementProperty:
                    exportMethodID = Naming.Method(componentType, Naming.GetterPrefix + name, false, ParameterTypeInfo.NoParams);
                    exportType = MethodBuilder.CreateDescriptor((target as CodeProperty).Type);
                    return true;

                case vsCMElement.vsCMElementClass:
                    //self export doesnt need exportMethodID
                    exportType = MethodBuilder.CreateDescriptor(target as CodeClass);
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
        /// <returns><c>true</c> if target has been succesfully found, <c>false</c> otherwise</returns>
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
                    importType = MethodBuilder.CreateDescriptor((target as CodeVariable).Type);
                    importMethodID = Naming.Method(componentType, Naming.SetterPrefix + name, false,
                        ParameterTypeInfo.Create("value", importType)
                        );
                    return true;

                case vsCMElement.vsCMElementProperty:
                    importType = MethodBuilder.CreateDescriptor((target as CodeProperty).Type);
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
            var method = getMethod(compositionAttrbiute.Element);
            if (method == null)
            {
                throw new NotImplementedException("Log that method cannot been loaded");
            }

            var info = MethodBuilder.CreateMethodInfo(method);

            var builder = getOrCreateCurrentBuilder(compositionAttrbiute.Element as CodeElement);
            builder.SetImportingCtor(info);
        }

        /// <summary>
        /// Add CompositionPoint according to given <see cref="CodeAttribute"/>
        /// </summary>
        /// <param name="compositionAttrbiute">Attribute defining export</param>
        private void addCompositionPoint(AttributeInfo compositionAttrbiute)
        {
            var method = getMethod(compositionAttrbiute.Element);
            if (method == null)
            {
                throw new NotImplementedException("Log that method cannot been loaded");
            }

            var info = MethodBuilder.CreateMethodInfo(method);

            var builder = getOrCreateCurrentBuilder(compositionAttrbiute.Element as CodeElement);
            builder.AddExplicitCompositionPoint(info.MethodID, createInitializer(compositionAttrbiute, info));
        }

        #region Literal parsing - TODO refactor out

        private string parseContract(string rawContract, ComponentInfoBuilder builder, CodeAttribute2 attribute)
        {
            if (rawContract == null)
                return null;

            var type = parseType(rawContract, builder, attribute);
            if (type != null)
                return type.TypeName;

            return parseString(rawContract);
        }

        private string parseString(string data)
        {
            if (data.StartsWith("@\"") && data.Length > 2)
                return data.Substring(2, data.Length - 3).Replace("\"\"", "\"");

            if (data.StartsWith("\"") && data.Length > 1)
                return data.Substring(1, data.Length - 2);

            return null;
        }

        private object parseObject(string data, ComponentInfoBuilder builder, CodeAttribute2 attribute)
        {
            object result;
            result = parseType(data, builder, attribute);

            if (result == null)
                result = parseString(data);

            if (result == null)
            {
                var value = parseBool(data);
                if (value.HasValue)
                    result = value.Value;
            }

            if (result != null)
                return result;

            char ch;
            if (char.TryParse(data, out ch))
                return ch;

            int intNum;
            if (int.TryParse(data, out intNum))
                return intNum;

            double doubNum;
            if (double.TryParse(data, out doubNum))
                return doubNum;

            return null;
        }

        private bool? parseBool(string data)
        {
            if (data == "true" || data == "false")
                return data == "true";

            return null;
        }

        private TypeDescriptor parseType(string data, ComponentInfoBuilder builder, CodeAttribute2 attribute)
        {
            var typePrefix = "typeof(";
            if (data.StartsWith(typePrefix))
            {
                data = data.Substring(typePrefix.Length).Replace(")", "");
                data = VsProjectAssembly.TranslatePath(data);

                //find contracted type

                var implicitNamespaces = _assembly.GetImplicitNamespaces(builder.ComponentType);

                var namespaces = implicitNamespaces.Concat(_assembly.GetNamespaces(attribute as CodeElement));
                foreach (var ns in namespaces)
                {
                    var prefix = ns == "" ? "" : ns + ".";

                    var descriptor = TypeDescriptor.Create(prefix + data);
                    if (_services.GetChain(descriptor) != null)
                        return descriptor;
                }
            }

            return null;
        }

        #endregion

        private GeneratorBase createInitializer(AttributeInfo compositionAttribute, TypeMethodInfo compositionPointInfo)
        {
            //TODO refactor into VsProjectAssembly


            if (compositionPointInfo.Parameters.Length == 0)
                //no arguments are required
                return null;

            if (compositionPointInfo.Parameters.Length != compositionAttribute.PositionalArgumentsCount)
                //TODO add special logging
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
                _builtComponents[currentClass] = builder = new ComponentInfoBuilder(MethodBuilder.CreateDescriptor(currentClass));
            }

            return builder;
        }

        /// <summary>
        /// Determine that given component class satisfies requirements for having implicit
        /// composition point
        /// </summary>
        /// <param name="componentClass">Class to be tested</param>
        /// <returns><c>true</c> if component class should have implicit composition point, <c>false</c> otherwise</returns>
        private bool couldHaveImplicitCompositionPoint(CodeClass2 componentClass)
        {
            var hasImplicitParamLessCtor = true;

            foreach (var member in componentClass.Members)
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
