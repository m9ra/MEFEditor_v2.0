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
    /// <summary>
    /// Type definition used for testing purposes.
    /// </summary>
    public class ManyStringImport : DataTypeDefinition
    {
        /// <summary>
        /// Member field representation.
        /// </summary>
        protected Field<string[]> Import;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManyStringImport"/> class.
        /// </summary>
        public ManyStringImport()
        {
            FullName = "ManyStringImport";

            var builder = new ComponentInfoBuilder(GetTypeInfo());
            var stringInfo = TypeDescriptor.Create<string>();
            var manyInfo = TypeDescriptor.Create<string[]>();

            builder.AddManyImport(manyInfo, stringInfo, "Import");
            ComponentInfo = builder.BuildWithImplicitCtor();
        }

        /// <summary>
        /// Member representation.
        /// </summary>
        public void _method_ctor()
        {
            Import.Set(new string[0]);
        }

        /// <summary>
        /// Member representation.
        /// </summary>
        /// <param name="import">The import.</param>
        public void _set_Import(string[] import)
        {
            Import.Set(import);
        }

        /// <summary>
        /// Member representation.
        /// </summary>
        /// <returns>System.String[].</returns>
        public string[] _get_Import()
        {
            return Import.Get();
        }

        /// <summary>
        /// Draws the specified services.
        /// </summary>
        /// <param name="services">The services.</param>
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
