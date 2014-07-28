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
    /// Analyzing definition of <see cref="ComposablePartCatalogCollection" />.
    /// </summary>
    public class ComposablePartCatalogCollectionDefinition : DataTypeDefinition
    {
        /// <summary>
        /// The type fullname.
        /// </summary>
        public const string TypeFullname = "System.ComponentModel.Composition.Hosting.ComposablePartCatalogCollection";

        /// <summary>
        /// The parent of collection.
        /// </summary>
        protected Field<Instance> Parent;

        /// <summary>
        /// The contained catalogs.
        /// </summary>
        protected Field<List<Instance>> Catalogs;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComposablePartCatalogCollectionDefinition" /> class.
        /// </summary>
        public ComposablePartCatalogCollectionDefinition()
        {
            FullName = TypeFullname;
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public void _method_ctor(Instance parent)
        {
            Catalogs.Set(new List<Instance>());
            Parent.Set(parent);
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        /// <param name="partCatalog">The part catalog.</param>
        [ParameterTypes(typeof(ComposablePartCatalog))]
        public void _method_Add(Instance partCatalog)
        {
            Catalogs.Get().Add(partCatalog);
            ReportChildAdd(Parent.Get(), 1, "Part catalog");
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        /// <returns>Instance[].</returns>
        public Instance[] _method_ToArray()
        {
            return Catalogs.Get().ToArray();
        }
    }
}
