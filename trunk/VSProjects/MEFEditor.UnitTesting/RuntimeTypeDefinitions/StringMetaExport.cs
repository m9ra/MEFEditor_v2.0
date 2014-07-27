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
    public class StringMetaExport : DataTypeDefinition
    {
        protected Field<string> Export;

        public StringMetaExport()
        {
            FullName = "StringMetaExport";

            var builder = new ComponentInfoBuilder(GetTypeInfo());
            builder.AddMeta("Key1", "Exported metadata1", true);
            builder.AddMeta("Key1", "Exported metadata2", true);
            builder.AddPropertyExport(TypeDescriptor.Create<string>(), "Export");
            builder.AddExplicitCompositionPoint(Naming.Method(TypeInfo, Naming.CtorName, false));

            ComponentInfo = builder.BuildWithImplicitCtor();
        }

        public void _method_ctor()
        {
            Export.Set("SimpleMetaExportValue");
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
