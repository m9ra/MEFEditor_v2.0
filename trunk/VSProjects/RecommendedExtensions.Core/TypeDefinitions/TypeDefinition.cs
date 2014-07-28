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
    /// Analyzing definition of <see cref="Type" />.
    /// </summary>
    public class TypeDefinition : DataTypeDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeDefinition" /> class.
        /// </summary>
        public TypeDefinition()
        {
            Simulate<Type>();
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
        /// <returns>System.String.</returns>
        public string _get_FullName()
        {
            var storedType = Context.GetField(This, "Type") as TypeDescriptor;

            if (storedType != null)
                return storedType.TypeName;

            return typeof(Type).FullName;
        }
    }
}
