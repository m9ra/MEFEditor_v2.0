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
    public class ComposablePartCatalogCollectionDefinition : DataTypeDefinition
    {
        public const string TypeFullname = "System.ComponentModel.Composition.Hosting.ComposablePartCatalogCollection";

        protected Field<Instance> Parent;

        protected Field<List<Instance>> Catalogs;

        public ComposablePartCatalogCollectionDefinition()
        {
            FullName = TypeFullname;
        }

        public void _method_ctor(Instance parent)
        {
            Catalogs.Set(new List<Instance>());
            Parent.Set(parent);
        }

        [ParameterTypes(typeof(ComposablePartCatalog))]
        public void _method_Add(Instance partCatalog)
        {
            Catalogs.Get().Add(partCatalog);
            ReportChildAdd(Parent.Get(), 1, "Part catalog");
        }

        public Instance[] _method_ToArray()
        {
            return Catalogs.Get().ToArray();
        }
    }
}
