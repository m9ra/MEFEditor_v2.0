using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;
using MEFEditor.TypeSystem;
using MEFEditor.TypeSystem.Runtime;

namespace MEFEditor.UnitTesting.RuntimeTypeDefinitions
{
    /// <summary>
    /// Type definition used for testing purposes.
    /// </summary>
    public class SimpleStringExport : DataTypeDefinition
    {
        /// <summary>
        /// Member field representation.
        /// </summary>
        protected Field<string> Export;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleStringExport"/> class.
        /// </summary>
        public SimpleStringExport()
        {
            FullName = "SimpleStringExport";

            var builder = new ComponentInfoBuilder(GetTypeInfo());
            builder.AddPropertyExport(TypeDescriptor.Create<string>(), "Export");
            builder.AddExplicitCompositionPoint(Naming.Method(TypeInfo, Naming.CtorName, false));

            ComponentInfo = builder.BuildWithImplicitCtor();
        }

        /// <summary>
        /// Member representation.
        /// </summary>
        public void _method_ctor()
        {
            Export.Set("SimpleExportValue");
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
