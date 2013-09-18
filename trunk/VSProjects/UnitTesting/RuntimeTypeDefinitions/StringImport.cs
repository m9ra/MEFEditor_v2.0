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
    public class StringImport : DataTypeDefinition
    {
        public readonly Field<string> Import;

        public StringImport()
        {
            Import = new Field<string>(this);
            FullName = "StringImport";

            var builder = new ComponentInfoBuilder(GetTypeInfo());
            builder.AddImport(InstanceInfo.Create<string>(), "Import");
            ComponentInfo = builder.BuildInfo();
        }

        public void _method_ctor()
        {
            Import.Set("Uninitialized import");
        }

        public void _set_Import(string import)
        {
            Import.Set(import);
        }

        public string _get_Import()
        {
            return Import.Get();
        }
    }
}
