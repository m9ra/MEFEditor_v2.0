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

            var exports = new Export[] { new Export(InstanceInfo.Create<string>()) };
            ComponentInfo = new ComponentInfo(new Import[0], exports, new Export[0]);
        }

        public void _method_ctor(string toExport)
        {
            Export.Set(toExport);
        }

        public string _get_Import()
        {
            return Export.Get();
        }
    }
}
