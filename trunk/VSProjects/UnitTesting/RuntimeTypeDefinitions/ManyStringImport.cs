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
        public readonly Field<string[]> Import;

        public ManyStringImport()
        {
            Import = new Field<string[]>(this);
            FullName = "ManyStringImport";

            var builder = new ComponentInfoBuilder(GetTypeInfo());
            var stringInfo = InstanceInfo.Create<string>();
            builder.AddManyImport(stringInfo, stringInfo, "Import");
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

        protected override bool tryDraw(DrawingServices services)
        {
            var importedValues = Import.Get();

            var i=0;
            foreach (var value in importedValues)
            {
                services.CurrentDrawing.SetProperty("Import[" + i + "]", value);
                ++i;
            }

            return true;
        }
    }
}
