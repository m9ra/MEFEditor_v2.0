using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Drawing;
using Analyzing;
using TypeSystem;
using TypeSystem.Runtime;

namespace UnitTesting.RuntimeTypeDefinitions
{
    public class StringExport : DataTypeDefinition
    {
        protected Field<string> Export;

        public StringExport()
        {
            FullName = "StringExport";

            var builder = new ComponentInfoBuilder(GetTypeInfo());
            builder.AddExport(TypeDescriptor.Create<string>(), "Export");
            builder.SetImportingCtor(TypeDescriptor.Create<string>());
            ComponentInfo = builder.BuildInfo();
        }

        public void _method_ctor(string toExport)
        {
            Export.Set("Data:" + toExport);
        }

        public void _method_ctor()
        {
            Export.Set("DefaultExport");
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
