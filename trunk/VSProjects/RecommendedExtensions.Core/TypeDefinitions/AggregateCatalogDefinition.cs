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

namespace RecommendedExtensions.Core.TypeDefinitions
{
    /// <summary>
    /// Analyzing definition of <see cref="AggregateCatalog" />.
    /// </summary>
    public class AggregateCatalogDefinition : DataTypeDefinition
    {
        /// <summary>
        /// The catalogs.
        /// </summary>
        protected Field<Instance> Catalogs;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateCatalogDefinition" /> class.
        /// </summary>
        public AggregateCatalogDefinition()
        {
            Simulate<AggregateCatalog>();
            AddCreationEdit("Add AggregateCatalog");
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        public void _method_ctor()
        {
            var catalogsInstance = Context.Machine.CreateInstance(TypeDescriptor.Create(ComposablePartCatalogCollectionDefinition.TypeFullname));
            Catalogs.Set(catalogsInstance);

            AddCallEdit(UserInteraction.AcceptEditName, acceptCatalog);

            //call list constructor
            AsyncCall<Instance>(catalogsInstance, Naming.CtorName, null, This);
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        /// <returns>Instance.</returns>
        [ReturnType(ComposablePartCatalogCollectionDefinition.TypeFullname)]
        public Instance _get_Catalogs()
        {
            return Catalogs.Get();
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        /// <returns>Instance[].</returns>
        public Instance[] _get_Parts()
        {
            AsyncCall<Instance[]>(Catalogs.Get(), "ToArray",
                returnParts
                );

            //result will be overriden
            return new Instance[0];
        }

        /// <summary>
        /// Set return value to parts in given catalogs.
        /// </summary>
        /// <param name="catalogs">The catalogs.</param>
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

        /// <summary>
        /// Handler for accept catalog edit.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <returns>CallEditInfo.</returns>
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

        /// <summary>
        /// Export data from represented <see cref="Instance" /> by using given drawer.
        /// <remarks>Note that only instances which are forced to display are displayed in root of <see cref="DiagramCanvas" /></remarks>
        /// </summary>
        /// <param name="drawer">The drawer.</param>
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
