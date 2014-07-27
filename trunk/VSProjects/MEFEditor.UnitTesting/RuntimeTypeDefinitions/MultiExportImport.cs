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
    public class MultiExportImport : DataTypeDefinition
    {
        protected Field<string> Export;

        public MultiExportImport()
        {
            FullName = "MultiExportImport";

            var builder = new ComponentInfoBuilder(GetTypeInfo());
            builder.AddPropertyExport(TypeDescriptor.Create<string>(), "Export");
            builder.AddPropertyExport(TypeDescriptor.Create<string>(), "Export2");

            builder.AddPropertyImport(TypeDescriptor.Create<string>(), "Import");
            builder.AddPropertyImport(TypeDescriptor.Create<string>(), "Import2");
            builder.AddExplicitCompositionPoint(Naming.Method(TypeInfo, Naming.CtorName, false));


            builder.AddSelfExport(FullName);
            builder.AddSelfExport(FullName);
            ComponentInfo = builder.BuildWithImplicitCtor();
        }

        public void _method_ctor()
        {
            Export.Set("SimpleExportValue");
        }

        public string _get_Export()
        {
            return Export.Get();
        }

        protected override void draw(InstanceDrawer services)
        {
            services.PublishField("Export", Export);
            services.ForceShow();
        }
    }
}
