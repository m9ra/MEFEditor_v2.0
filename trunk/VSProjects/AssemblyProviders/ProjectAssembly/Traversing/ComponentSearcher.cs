using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnvDTE;
using EnvDTE80;

using TypeSystem;
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
        /// Stack of currently searched <see cref="CodeClass"/> objects
        /// </summary>
        private readonly Stack<CodeClass> _classStack = new Stack<CodeClass>();

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
            _classStack.Push(e);
            base.VisitClass(e);
            var popped = _classStack.Pop();

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
            var fullname = e.FullName;

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
                throw new NotImplementedException("Add composition point attribute");
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
            var property = getProperty(importAttrbute.Element);
            if (property == null)
            {
                //TODO log that import attribute cannot be handled
                return;
            }

            var name = property.Name;
            var type = property.Type;
            var explicitContract = importAttrbute.GetArgument(0);

            var allowMany = forceMany || importAttrbute.IsTrue("AllowMany");
            var allowDefault = forceMany || importAttrbute.IsTrue("AllowDefault");

            var importTypeDesciptor = MethodBuilder.CreateDescriptor(type);
            var importTypeInfo = ImportTypeInfo.ParseFromMany(importTypeDesciptor, allowMany, _services);
            var contract = explicitContract == null ? importTypeInfo.ItemType.TypeName : explicitContract;

            var builder = getOrCreateCurrentBuilder();
            var setterID = Naming.Method(builder.ComponentType, Naming.SetterPrefix + name, false, ParameterTypeInfo.Create("p", importTypeDesciptor));
            builder.AddImport(importTypeInfo, setterID, contract, allowMany, allowDefault);
        }

        /// <summary>
        /// Add export according to given <see cref="CodeAttribute"/>
        /// </summary>
        /// <param name="importAttrbiute">Attribute defining export</param>
        private void addExport(AttributeInfo exportAttrbiute)
        {
            var property = getProperty(exportAttrbiute.Element);
            if (property == null)
            {
                //TODO log that export attribute cannot be handled
                throw new NotImplementedException("Check for self exports");
            }

            var name = property.Name;
            var type = property.Type;

            var explicitContract = exportAttrbiute.GetArgument(0);
            var exportTypeDescriptor = MethodBuilder.CreateDescriptor(type);
            var contract = explicitContract == null ? exportTypeDescriptor.TypeName : explicitContract;

            var builder = getOrCreateCurrentBuilder();
            var getterID = Naming.Method(builder.ComponentType, Naming.GetterPrefix + name, false, ParameterTypeInfo.NoParams);
            builder.AddExport(exportTypeDescriptor, getterID, contract);
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
        /// Get or create <see cref="ComponentInfoBuilder"/> for currently visited class
        /// </summary>
        /// <returns><see cref="ComponentInfoBuilder"/> for currently visited class</returns>
        private ComponentInfoBuilder getOrCreateCurrentBuilder()
        {
            var currentClass = _classStack.Peek();

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
