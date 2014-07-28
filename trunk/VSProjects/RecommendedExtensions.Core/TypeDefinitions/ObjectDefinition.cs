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
    /// Analyzing definition of <see cref="object" />.
    /// </summary>
    public class ObjectDefinition : DataTypeDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDefinition"/> class.
        /// </summary>
        public ObjectDefinition()
        {
            Simulate<object>();
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
        /// <returns>Instance.</returns>
        public Instance _method_GetType()
        {
            var typeInfo = TypeDescriptor.Create<Type>();
            var result = Context.Machine.CreateInstance(typeInfo);
            Context.SetField(result, "Type", This.Info);

            return result;
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        /// <returns>System.String.</returns>
        public string _method_ToString()
        {
            return This.Info.TypeName;
        }
    }
}
