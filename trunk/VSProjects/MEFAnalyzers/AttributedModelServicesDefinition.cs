using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

using Analyzing;
using Analyzing.Editing;
using TypeSystem;
using TypeSystem.Runtime;

using MEFEditor.Drawing;

namespace MEFAnalyzers
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
