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
    /// <summary>
    /// Analyzing definition of <see cref="Console" />.
    /// </summary>
    public class ConsoleDefinition : DataTypeDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleDefinition"/> class.
        /// </summary>
        public ConsoleDefinition()
        {
            FullName = "System.Console";
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        public void _method_ctor()
        {
            //nothing to do
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        public void _static_method_cctor()
        {
            //nothing to do
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="formatArgs">The format arguments.</param>
        public void _method_Write(string message, params Instance[] formatArgs)
        {
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="formatArgs">The format arguments.</param>
        public void _method_WriteLine(string message, params Instance[] formatArgs)
        {
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        public void _method_ReadKey()
        {
        }
    }
}
