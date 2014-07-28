using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

using MEFEditor.Analyzing;
using MEFEditor.Analyzing.Editing;
using MEFEditor.TypeSystem;
using MEFEditor.TypeSystem.Runtime;

using MEFEditor.Drawing;

using RecommendedExtensions.Core.Dialogs;

namespace RecommendedExtensions.Core.TypeDefinitions
{
    /// <summary>
    /// Analyzing definition of <see cref="TypeCatalog" />.
    /// </summary>
    public class TypeCatalogDefinition : DataTypeDefinition
    {
        /// <summary>
        /// The contained parts.
        /// </summary>
        protected Field<List<Instance>> Parts;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeCatalogDefinition" /> class.
        /// </summary>
        public TypeCatalogDefinition()
        {
            AddCreationEdit("Add TypeCatalog");
            Simulate<TypeCatalog>();
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        /// <param name="types">The types.</param>
        [ParameterTypes(typeof(Type[]))]
        public void _method_ctor(params Instance[] types)
        {
            var parts = new List<Instance>();
            Parts.Set(parts);

            //offer components available in place of calling - they can be added to ctor
            TypeAssembly callerAssembly = GetCallerAssembly();
            Edits.AppendArgument(This, types.Length + 1, "Add component type", (v) => addComponentTypeProvider(callerAssembly, v));

            //keeping in closure
            var edits = Edits;
            var thisInst = This;

            //collect names of types
            for (var i = 0; i < types.Length; ++i)
            {
                //because of keeping values in closure
                var index = i;
                var type = types[index];

                AsyncCall<string>(type, "get_FullName", (fullname) =>
                {
                    var info = TypeDescriptor.Create(fullname);
                    var part = Context.Machine.CreateInstance(info);

                    edits.SetOptional(index + 1);
                    edits.AttachRemoveArgument(thisInst, part, index + 1, "Remove component type");

                    parts.Add(part);
                });
            }
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        /// <returns>Instance[].</returns>
        public Instance[] _get_Parts()
        {
            return Parts.Get().ToArray();
        }

        /// <summary>
        /// Export data from represented <see cref="Instance" /> by using given drawer.
        /// <remarks>Note that only instances which are forced to display are displayed in root of <see cref="DiagramCanvas" /></remarks>.
        /// </summary>
        /// <param name="drawer">The drawer.</param>
        protected override void draw(InstanceDrawer drawer)
        {
            var slot = drawer.AddSlot();

            var parts = Parts.Get();
            if (parts != null)
            {
                foreach (var part in parts)
                {
                    var partDrawing = drawer.GetInstanceDrawing(part);
                    slot.Add(partDrawing.Reference);
                }
            }

            drawer.ForceShow();
        }

        /// <summary>
        /// Dialog for adding component type.
        /// </summary>
        /// <param name="callerAssembly">The caller assembly.</param>
        /// <param name="v">View where component type will be added.</param>
        /// <returns>System.Object.</returns>
        private object addComponentTypeProvider(TypeAssembly callerAssembly, ExecutionView v)
        {
            var components = getComponents(callerAssembly);
            var dialog = new ComponentType(components);

            if (dialog.ShowDialog() == true)
            {
                return dialog.SelectedComponent;
            }
            else
            {
                v.Abort("No component has been selected");
                return null;
            }
        }

        /// <summary>
        /// Gets components defined in given assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>IEnumerable&lt;ComponentInfo&gt;.</returns>
        private IEnumerable<ComponentInfo> getComponents(TypeAssembly assembly)
        {
            var result = new List<ComponentInfo>(assembly.GetReferencedComponents());
            
            //runtime is implicitly referenced from every assembly 
            var runtimeComponents = Services.GetComponents(ContainingAssembly);
            result.AddRange(runtimeComponents);

            return result;
        }
    }
}
