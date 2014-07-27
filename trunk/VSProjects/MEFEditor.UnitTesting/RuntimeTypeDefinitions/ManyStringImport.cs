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
            ComponentInfo = builder.BuildWithImplicitCtor();
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
