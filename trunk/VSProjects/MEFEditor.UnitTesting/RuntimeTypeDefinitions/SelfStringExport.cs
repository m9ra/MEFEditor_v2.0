using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Drawing;
using MEFEditor.Analyzing;
using MEFEditor.TypeSystem;
using MEFEditor.TypeSystem.Runtime;

namespace MEFEditor.UnitTesting.RuntimeTypeDefinitions
{
    /// <summary>
    /// Type definition used for testing purposes.
    /// </summary>
    public class SelfStringExport : DataTypeDefinition
    {
        /// <summary>
        /// Member field representation.
        /// </summary>
        protected Field<string> Export;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelfStringExport" /> class.
        /// </summary>
        public SelfStringExport()
        {
            FullName = "SelfStringExport";
            var builder = new ComponentInfoBuilder(GetTypeInfo());
            builder.AddSelfExport(false,typeof(string).FullName);
            builder.SetImportingCtor(TypeDescriptor.Create<string>());
            ComponentInfo = builder.BuildWithImplicitCtor();
        }

        /// <summary>
        /// Member representation.
        /// </summary>
        /// <param name="toExport">To export.</param>
        public void _method_ctor(string toExport)
        {

        }
    }
}
