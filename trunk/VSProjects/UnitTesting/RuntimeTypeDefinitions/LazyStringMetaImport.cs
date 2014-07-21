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
    public class LazyStringMetaImport : DataTypeDefinition
    {
        public readonly Field<Lazy<InstanceWrap, InstanceWrap>> Import;

        public LazyStringMetaImport()
        {
            FullName = "LazyStringMetaImport";

            var builder = new ComponentInfoBuilder(GetTypeInfo());
            builder.AddPropertyImport(TypeDescriptor.Create("System.Lazy<System.String,MetaInterface>"), "Import");
            builder.SetImportingCtor();

            ComponentInfo = builder.BuildInfo();
        }

        public void _method_ctor()
        {
            var instance = Context.Machine.CreateDirectInstance("Uninitialized import");
            Import.Set(new Lazy<InstanceWrap, InstanceWrap>(() => new InstanceWrap(instance), null));
        }

        [ParameterTypes("System.Lazy<System.String,MetaInterface>")]
        public void _set_Import(Lazy<InstanceWrap, InstanceWrap> import)
        {
            Import.Set(import);
        }

        [ReturnType("System.Lazy<System.String,MetaInterface>")]
        public Lazy<InstanceWrap, InstanceWrap> _get_Import()
        {
            return Import.Value;
        }

        protected override void draw(InstanceDrawer services)
        {
            services.PublishField("Import", Import);
            services.SetProperty("ImportValue", Import.Value.Value.Wrapped.ToString());
            services.ForceShow();
        }
    }
}
