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
    public class ManyStringImport : DataTypeDefinition
    {
        protected Field<string[]> Import;

        public ManyStringImport()
        {
            FullName = "ManyStringImport";

            var builder = new ComponentInfoBuilder(GetTypeInfo());
            var stringInfo = TypeDescriptor.Create<string>();
            var manyInfo = TypeDescriptor.Create<string[]>();

            builder.AddManyImport(manyInfo, stringInfo, "Import");
            ComponentInfo = builder.BuildInfo();
        }

        public void _method_ctor()
        {
            Import.Set(new string[0]);
        }

        public void _set_Import(string[] import)
        {
            Import.Set(import);
        }

        public string[] _get_Import()
        {
            return Import.Get();
        }

        protected override void draw(InstanceDrawer services)
        {
            var importedValues = Import.Get();

            var i = 0;
            foreach (var value in importedValues)
            {
                services.SetProperty("Import[" + i + "]", value);
                ++i;
            }

            services.ForceShow();
        }
    }
}
