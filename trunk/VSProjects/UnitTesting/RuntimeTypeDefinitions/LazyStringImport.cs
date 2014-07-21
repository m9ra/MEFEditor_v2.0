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
    public class LazyStringImport : DataTypeDefinition
    {
        public readonly Field<Lazy<InstanceWrap>> Import;

        public LazyStringImport()
        {
            FullName = "LazyStringImport";

            var builder = new ComponentInfoBuilder(GetTypeInfo());
            builder.AddPropertyImport(TypeDescriptor.Create<Lazy<string>>(), "Import");
            builder.SetImportingCtor();

            ComponentInfo = builder.BuildInfo();
        }

        public void _method_ctor()
        {
            var instance = Context.Machine.CreateDirectInstance("Uninitialized import");
            Import.Set(new Lazy<InstanceWrap>(() => new InstanceWrap(instance)));
        }

        [ParameterTypes(typeof(Lazy<string>))]
        public void _set_Import(Lazy<InstanceWrap> import)
        {
            Import.Set(import);
        }

        [ReturnType(typeof(Lazy<string>))]
        public Lazy<InstanceWrap> _get_Import()
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
