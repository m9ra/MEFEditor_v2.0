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

namespace MEFAnalyzers
{
    public class TypeCatalogDefinition : DataTypeDefinition
    {
        protected Field<List<Instance>> Parts;

        public TypeCatalogDefinition()
        {
            Simulate<TypeCatalog>();
        }

        public void _method_ctor(params Instance[] types)
        {
            var parts = new List<Instance>();
            Parts.Set(parts);
            
            //collect names of types
            foreach (var type in types)
            {
                AsyncCall<string>(type, "get_FullName", (fullname) =>
                {
                    var info = new InstanceInfo(fullname);
                    var part = Context.Machine.CreateInstance(info);

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

            foreach (var part in Parts.Get())
            {
                var partDrawing = drawer.GetInstanceDrawing(part);
                slot.Add(partDrawing.Reference);
            }

            drawer.ForceShow();
        }
    }
}
