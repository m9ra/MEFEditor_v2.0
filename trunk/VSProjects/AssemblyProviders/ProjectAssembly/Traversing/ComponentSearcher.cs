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
        /// Indexes of builded components
        /// </summary>
        private Dictionary<CodeClass, ComponentInfoBuilder> _buildedComponents = new Dictionary<CodeClass, ComponentInfoBuilder>();

        /// <summary>
        /// Initialize instance of <see cref="ComponentSearcher"/>
        /// </summary>
        /// <param name="services"><see cref="TypeServices"/> used for resolving types' inheritance</param>
        internal ComponentSearcher(TypeServices services)
        {
            if (services == null)
                throw new ArgumentNullException("services");

            _services = services;
        }

        #region Visitor overrides

        /// <inheritdoc />
        public override void VisitClass(CodeClass2 e)
        {
            //keep stack of nesting classes
            base.VisitClass(e);

            if (_buildedComponents.ContainsKey(e))
            {
                //component has been found
                var componentInfo = _buildedComponents[e].BuildInfo();
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

            var explicitContract = importAttrbute.GetArgument(0);

            var allowMany = forceMany || importAttrbute.IsTrue("AllowMany");
            var allowDefault = forceMany || importAttrbute.IsTrue("AllowDefault");

            var importTypeInfo = ImportTypeInfo.ParseFromMany(importType, allowMany, _services);
            var contract = explicitContract == null ? importTypeInfo.ItemType.TypeName : explicitContract;

            builder.AddImport(importTypeInfo, importMethodID, contract, allowMany, allowDefault);
        }

        /// <summary>
        /// Add export according to given <see cref="CodeAttribute"/>
        /// </summary>
        /// <param name="exportAttrbiute">Attribute defining export</param>
        private void addExport(AttributeInfo exportAttrbiute)
        {
            var builder = getOrCreateCurrentBuilder(exportAttrbiute.Element as CodeElement);

            TypeDescriptor exportTypeDescriptor;
            MethodID exportMethodID;
            if (!getExportTarget(exportAttrbiute.Element, builder.ComponentType, out exportMethodID, out exportTypeDescriptor))
            {
                //TODO log that export attribute cannot be handled
                return;
            }

            var explicitContract = exportAttrbiute.GetArgument(0);
            var contract = explicitContract == null ? exportTypeDescriptor.TypeName : explicitContract;
            var isSelfExport = exportMethodID == null;

            if (isSelfExport)
            {
                builder.AddSelfExport(contract);
            }
            else
            {
                builder.AddExport(exportTypeDescriptor, exportMethodID, contract);
            }
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

            //TODO handle composition point arguments
            var info = MethodBuilder.CreateMethodInfo(method);

            var builder = getOrCreateCurrentBuilder(compositionAttrbiute.Element as CodeElement);
            builder.AddExplicitCompositionPoint(info.MethodID);
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
            if (!_buildedComponents.TryGetValue(currentClass, out builder))
            {
                _buildedComponents[currentClass] = builder = new ComponentInfoBuilder(MethodBuilder.CreateDescriptor(currentClass));
            }

            return builder;
        }

        #endregion
    }
}
