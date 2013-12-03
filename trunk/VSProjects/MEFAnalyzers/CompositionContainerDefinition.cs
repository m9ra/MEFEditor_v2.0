using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Composition.Hosting;

using Analyzing;
using TypeSystem;
using TypeSystem.Runtime;


namespace MEFAnalyzers
{
    public class CompositionContainerDefinition : DataTypeDefinition
    {
        public CompositionContainerDefinition()
        {
            Simulate<CompositionContainer>();
        }

        /// <summary>
        /// TODO ComposablePartCatalog restriction is needed
        /// </summary>
        /// <param name="composablePartCatalog"></param>
        public void _method_ctor(Instance composablePartCatalog)
        {
            throw new NotImplementedException();
        }

        public void _method_ComposeParts(params Instance[] parts)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// TODO composition batch restriction is needed
        /// </summary>
        public void _method_Compose(Instance batch)
        {
            throw new NotImplementedException();
        }
    }
}
