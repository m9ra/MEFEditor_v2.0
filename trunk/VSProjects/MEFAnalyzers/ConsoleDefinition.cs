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
    public class ConsoleDefinition : DataTypeDefinition
    {
        public ConsoleDefinition()
        {
            FullName = "System.Console";
        }

        public void _method_ctor()
        {
            //nothing to do
        }

        public void _static_method_cctor()
        {
            //nothing to do
        }

        public void _method_Write(string message, params Instance[] formatArgs)
        {
        }

        public void _method_WriteLine(string message, params Instance[] formatArgs)
        {
        }

        public void _method_ReadKey()
        {
        }
    }
}
