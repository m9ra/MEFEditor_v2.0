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
    public class ComposablePartCatalogDefinition : DataTypeDefinition
    {
        public ComposablePartCatalogDefinition()
        {
            Simulate<ComposablePartCatalog>();
        }
    }
}
