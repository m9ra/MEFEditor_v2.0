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
    public class MultiExportImport : DataTypeDefinition
    {
        /// <summary>
        /// Member field representation.
        /// </summary>
        protected Field<string> Export;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiExportImport" /> class.
        /// </summary>
        public MultiExportImport()
        {
            FullName = "MultiExportImport";

            var builder = new ComponentInfoBuilder(GetTypeInfo());
            builder.AddPropertyExport(TypeDescriptor.Create<string>(), "Export");
            builder.AddPropertyExport(TypeDescriptor.Create<string>(), "Export2");

            builder.AddPropertyImport(TypeDescriptor.Create<string>(), "Import");
            builder.AddPropertyImport(TypeDescriptor.Create<string>(), "Import2");
            builder.AddExplicitCompositionPoint(Naming.Method(TypeInfo, Naming.CtorName, false));


            builder.AddSelfExport(false, FullName);
            builder.AddSelfExport(false, FullName);
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
