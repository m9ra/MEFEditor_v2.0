using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

using MEFEditor.Analyzing;
using MEFEditor.Analyzing.Editing;
using MEFEditor.TypeSystem;
using MEFEditor.TypeSystem.Runtime;

using MEFEditor.Drawing;

namespace RecommendedExtensions.Core.TypeDefinitions
{
    public class AttributedModelServicesDefinition : DataTypeDefinition
    {
        public AttributedModelServicesDefinition()
        {
            //static class cannot be simulated
            FullName = "System.ComponentModel.Composition.AttributedModelServices";
        }

        [ParameterTypes(typeof(CompositionContainer),typeof(object[]))]
        public void _static_method_ComposeParts(Instance container, params Instance[] parts)
        {
            //Transfer call to composition container, 
            //because it can display it more accurate

            AsyncCall<Instance>(container, "ComposeParts", null, CurrentArguments[2]);
        }
    }
}
