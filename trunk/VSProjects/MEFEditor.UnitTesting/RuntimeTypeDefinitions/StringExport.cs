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
    public class StringExport : DataTypeDefinition
    {
        /// <summary>
        /// Member field representation.
        /// </summary>
        protected Field<string> Export;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringExport"/> class.
        /// </summary>
        public StringExport()
        {
            FullName = "StringExport";

            var builder = new ComponentInfoBuilder(GetTypeInfo());
            builder.AddPropertyExport(TypeDescriptor.Create<string>(), "Export");
            builder.SetImportingCtor(TypeDescriptor.Create<string>());            
            ComponentInfo = builder.BuildWithImplicitCtor();
        }

        /// <summary>
        /// Member representation.
        /// </summary>
        /// <param name="toExport">To export.</param>
        public void _method_ctor(string toExport)
        {
            Export.Set("Data:" + toExport);
        }

        /// <summary>
        /// Member representation.
        /// </summary>
        public void _method_ctor()
        {
            Export.Set("DefaultExport");
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
