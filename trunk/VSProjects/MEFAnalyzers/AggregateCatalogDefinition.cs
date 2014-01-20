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
        protected Field<Instance> Catalogs;

        public AggregateCatalogDefinition()
        {
            Simulate<AggregateCatalog>();
            AddCreationEdit("Add AggregateCatalog", Dialogs.VariableName.GetName);
        }

        public void _method_ctor()
        {
            var catalogsInstance = Context.Machine.CreateInstance(TypeDescriptor.Create(ComposablePartCatalogCollectionDefinition.TypeFullname));
            Catalogs.Set(catalogsInstance);

            AddCallEdit(UserInteraction.AcceptName, acceptCatalog);

            //call list constructor
            AsyncCall<Instance>(catalogsInstance, Naming.CtorName, null, This);
        }

        [ReturnType(ComposablePartCatalogCollectionDefinition.TypeFullname)]
        public Instance _get_Catalogs()
        {
            return Catalogs.Get();
        }

        public Instance[] _get_Parts()
        {
            AsyncCall<Instance[]>(Catalogs.Get(), "ToArray",
                returnParts
                );

            //result will be overriden
            return new Instance[0];
        }

        private void returnParts(Instance[] catalogs)
        {
            var collectedParts = new HashSet<Instance>();
            foreach (var catalog in catalogs)
            {
                AsyncCall<Instance[]>(catalog, "get_Parts", (parts) =>
                {
                    collectedParts.UnionWith(parts);
                });
            }

            ContinuationCall((context) =>
            {
                var array = Wrap(context, collectedParts.ToArray());
                context.Return(array);
            });
        }

        private CallEditInfo acceptCatalog(ExecutionView view)
        {
            var instance = UserInteraction.DraggedInstance;
            var isCatalog = Services.IsAssignable(TypeDescriptor.Create<ComposablePartCatalog>(), instance.Info);

            if (!isCatalog)
            {
                view.Abort("AggregateCatalog can only accept part catalogs");
                return null;
            }

            return new CallEditInfo(This, "Catalogs.Add", instance);
        }

        protected override void draw(InstanceDrawer drawer)
        {
            var slot = drawer.AddSlot();
            foreach (var child in Children)
            {
                var childDrawing = drawer.GetInstanceDrawing(child);
                slot.Add(childDrawing.Reference);
            }

            drawer.ForceShow();
        }
    }
}
