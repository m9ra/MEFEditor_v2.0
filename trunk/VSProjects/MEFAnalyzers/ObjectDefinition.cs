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

using Drawing;

namespace MEFAnalyzers
{
    public class ObjectDefinition : DataTypeDefinition
    {
        public ObjectDefinition()
        {
            Simulate<object>();
        }

        public void _method_ctor()
        {
            //nothing to do
        }
        
        public void _static_method_cctor()
        {
            //nothing to do
        }
    }
}
