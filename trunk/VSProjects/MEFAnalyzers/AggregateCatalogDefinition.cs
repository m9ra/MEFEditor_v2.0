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
            var catalogsInstance = Context.Machine.CreateInstance(new InstanceInfo(ComposablePartCatalogCollectionDefinition.TypeFullname));
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
            throw new NotImplementedException();
        }

        private CallEditInfo acceptCatalog(ExecutionView view)
        {
            var instance = UserInteraction.DraggedInstance;
            var isCatalog = Services.IsAssignable(InstanceInfo.Create<ComposablePartCatalog>(), instance.Info);

            if (!isCatalog)
            {
                //allow accepting only components
                view.Abort("CompositionContainer can only accept part catalogs");
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
