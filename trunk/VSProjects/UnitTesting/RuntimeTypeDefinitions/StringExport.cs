using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using TypeSystem;
using TypeSystem.Runtime;

namespace UnitTesting.RuntimeTypeDefinitions
{
    public class StringExport : DataTypeDefinition
    {
        public readonly Field<string> Export;

        public StringExport()
        {
            Export = new Field<string>(this);
            FullName = "StringExport";

            var builder = new ComponentInfoBuilder(GetTypeInfo());
            builder.AddExport(InstanceInfo.Create<string>(), "Export");
            ComponentInfo = builder.BuildInfo();
        }

        public void _method_ctor(string toExport)
        {
            Export.Set(toExport);
        }

        public string _get_Export()
        {
            return Export.Get();
        }
    }
}
