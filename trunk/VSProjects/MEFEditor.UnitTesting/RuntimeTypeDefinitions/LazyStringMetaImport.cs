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
    public class LazyStringMetaImport : DataTypeDefinition
    {
        /// <summary>
        /// Member field representation.
        /// </summary>
        public readonly Field<Lazy<InstanceWrap, InstanceWrap>> Import;

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyStringMetaImport"/> class.
        /// </summary>
        public LazyStringMetaImport()
        {
            FullName = "LazyStringMetaImport";

            var builder = new ComponentInfoBuilder(GetTypeInfo());
            builder.AddPropertyImport(TypeDescriptor.Create("System.Lazy<System.String,MetaInterface>"), "Import");
            builder.SetImportingCtor();

            ComponentInfo = builder.BuildWithImplicitCtor();
        }

        /// <summary>
        /// Member representation.
        /// </summary>
        public void _method_ctor()
        {
            var instance = Context.Machine.CreateDirectInstance("Uninitialized import");
            Import.Set(new Lazy<InstanceWrap, InstanceWrap>(() => new InstanceWrap(instance), null));
        }

        /// <summary>
        /// Member representation.
        /// </summary>
        /// <param name="import">The import.</param>
        [ParameterTypes("System.Lazy<System.String,MetaInterface>")]
        public void _set_Import(Lazy<InstanceWrap, InstanceWrap> import)
        {
            Import.Set(import);
        }

        /// <summary>
        /// Member representation.
        /// </summary>
        /// <returns>Lazy&lt;InstanceWrap, InstanceWrap&gt;.</returns>
        [ReturnType("System.Lazy<System.String,MetaInterface>")]
        public Lazy<InstanceWrap, InstanceWrap> _get_Import()
        {
            return Import.Value;
        }

        /// <summary>
        /// Draws the specified services.
        /// </summary>
        /// <param name="services">The services.</param>
        protected override void draw(InstanceDrawer services)
        {
            services.PublishField("Import", Import);
            services.SetProperty("ImportValue", Import.Value.Value.Wrapped.ToString());
            services.ForceShow();
        }
    }
}
