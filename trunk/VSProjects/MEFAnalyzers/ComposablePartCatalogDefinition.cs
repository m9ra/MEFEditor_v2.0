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

using MEFEditor.Drawing;

namespace MEFAnalyzers
{
    public class ComposablePartCatalogDefinition : DataTypeDefinition
    {
        public ComposablePartCatalogDefinition()
        {
            Simulate<ComposablePartCatalog>();
        }
    }
}
