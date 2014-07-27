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
    public class StringExport2 : DataTypeDefinition
    {
        protected Field<string> Export;

        public StringExport2()
        {
            FullName = "StringExport2";

            var builder = new ComponentInfoBuilder(GetTypeInfo());
            builder.AddPropertyExport(TypeDescriptor.Create<string>(), "Export");
            ComponentInfo = builder.BuildWithImplicitCtor();
        }
        
        public void _method_ctor()
        {
            Export.Set("Data2:DefaultExport");
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
