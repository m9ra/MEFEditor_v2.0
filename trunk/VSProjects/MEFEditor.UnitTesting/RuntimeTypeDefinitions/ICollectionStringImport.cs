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
    public class ICollectionStringImport : DataTypeDefinition
    {
        /// <summary>
        /// Member field representation.
        /// </summary>
        protected Field<List<string>> Import;

        /// <summary>
        /// Initializes a new instance of the <see cref="ICollectionStringImport"/> class.
        /// </summary>
        public ICollectionStringImport()
        {
            FullName = "ICollectionStringImport";

            var builder = new ComponentInfoBuilder(GetTypeInfo());
            builder.AddManyImport(TypeDescriptor.Create<List<string>>(), TypeDescriptor.Create<string>(), "Import");
            ComponentInfo = builder.BuildWithImplicitCtor();
        }

        /// <summary>
        /// Member representation.
        /// </summary>
        public void _method_ctor()
        {
            var data = new List<string>();
            //   data.Add("Uninitialized import");
            Import.Set(new List<string>(data));
        }

        /// <summary>
        /// Member representation.
        /// </summary>
        /// <param name="import">The import.</param>
        public void _set_Import(List<string> import)
        {
            Import.Set(import);
        }

        /// <summary>
        /// Member representation.
        /// </summary>
        /// <returns>List&lt;System.String&gt;.</returns>
        public List<string> _get_Import()
        {
            return Import.Get();
        }

        /// <summary>
        /// Export data from represented <see cref="Instance" /> by using given drawer.
        /// <remarks>Note that only instances which are forced to display are displayed in root of <see cref="MEFEditor.Drawing.DiagramCanvas" /></remarks>
        /// </summary>
        /// <param name="drawer">The drawer.</param>
        protected override void draw(InstanceDrawer drawer)
        {
            var importedValues = Import.Get();

            if (importedValues != null)
            {
                var i = 0;
                foreach (var value in importedValues)
                {
                    drawer.SetProperty("Import[" + i + "]", value);
                    ++i;
                }
            }

            drawer.ForceShow();
        }
    }
}
