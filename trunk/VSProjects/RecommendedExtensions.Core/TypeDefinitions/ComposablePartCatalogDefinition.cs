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
    /// Analyzing definition of <see cref="ComposasblePartCatalog" />.
    /// </summary>
    public class ComposablePartCatalogDefinition : DataTypeDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComposablePartCatalogDefinition"/> class.
        /// </summary>
        public ComposablePartCatalogDefinition()
        {
            Simulate<ComposablePartCatalog>();
        }
    }
}
