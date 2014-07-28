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
    public class StringExport2 : DataTypeDefinition
    {
        /// <summary>
        /// Member field representation.
        /// </summary>
        protected Field<string> Export;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringExport2"/> class.
        /// </summary>
        public StringExport2()
        {
            FullName = "StringExport2";

            var builder = new ComponentInfoBuilder(GetTypeInfo());
            builder.AddPropertyExport(TypeDescriptor.Create<string>(), "Export");
            ComponentInfo = builder.BuildWithImplicitCtor();
        }

        /// <summary>
        /// Member representation.
        /// </summary>
        public void _method_ctor()
        {
            Export.Set("Data2:DefaultExport");
        }

        /// <summary>
        /// Member representation.
        /// </summary>
        /// <returns>System.String.</returns>
        public string _get_Export()
        {
            return Export.Get();
        }

        /// <summary>
        /// Draws the specified services.
        /// </summary>
        /// <param name="services">The services.</param>
        protected override void draw(InstanceDrawer services)
        {
            services.PublishField("Export", Export);
            services.ForceShow();
        }
    }
}
