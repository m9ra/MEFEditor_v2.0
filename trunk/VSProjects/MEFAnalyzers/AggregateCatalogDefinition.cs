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
    public class AggregateCatalogDefinition : DataTypeDefinition
    {
        Field<Instance> Catalogs;

        public AggregateCatalogDefinition()
        {
            Simulate<AggregateCatalog>();

            Catalogs = new Field<Instance>(this);
        }

        public void _method_ctor()
        {
            var catalogsInstance = Context.Machine.CreateInstance(new InstanceInfo(ComposablePartCatalogCollectionDefinition.TypeFullname));

            AddCallEdit(".accept", acceptCatalog);

            //call list constructor
            AsyncCall<Instance>(catalogsInstance, Naming.CtorName, null, This);
            Catalogs.Set(catalogsInstance);
        }

        [ReturnType(ComposablePartCatalogCollectionDefinition.TypeFullname)]
        public Instance _get_Catalogs()
        {
            return Catalogs.Get();
        }

        private CallEditInfo acceptCatalog(ExecutionView view)
        {
            //TODO check for accepting only ComposableParts
            return new CallEditInfo(This, "Catalogs.Add", UserInteraction.DraggedInstance);
        }

        protected override void draw(InstanceDrawer drawer)
        {
            var slot = drawer.AddSlot();
            foreach (var child in Children)
            {
                var childDrawing = drawer.GetInstanceDrawing(child);
                slot.Add(childDrawing.Reference);
            }

            drawer.CommitDrawing();
        }
    }
}
