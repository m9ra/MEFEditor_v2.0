using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

using Analyzing;
using Analyzing.Editing;
using TypeSystem;
using TypeSystem.Runtime;

using Drawing;

using MEFAnalyzers.Dialogs;

namespace MEFAnalyzers
{
    public class TypeCatalogDefinition : DataTypeDefinition
    {
        protected Field<List<Instance>> Parts;

        public TypeCatalogDefinition()
        {
            AddCreationEdit("Add TypeCatalog", Dialogs.VariableName.GetName);
            Simulate<TypeCatalog>();
        }

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

        public Instance[] _get_Parts()
        {
            return Parts.Get().ToArray();
        }

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
