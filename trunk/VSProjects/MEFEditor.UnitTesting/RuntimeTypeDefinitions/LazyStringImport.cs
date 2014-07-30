﻿using System;
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
    public class LazyStringImport : DataTypeDefinition
    {
        /// <summary>
        /// Member field representation.
        /// </summary>
        public readonly Field<Lazy<InstanceWrap>> Import;

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyStringImport"/> class.
        /// </summary>
        public LazyStringImport()
        {
            FullName = "LazyStringImport";

            var builder = new ComponentInfoBuilder(GetTypeInfo());
            builder.AddPropertyImport(TypeDescriptor.Create<Lazy<string>>(), "Import");
            builder.SetImportingCtor();

            ComponentInfo = builder.BuildWithImplicitCtor();
        }

        /// <summary>
        /// Member representation.
        /// </summary>
        public void _method_ctor()
        {
            var instance = Context.Machine.CreateDirectInstance("Uninitialized import");
            Import.Set(new Lazy<InstanceWrap>(() => new InstanceWrap(instance)));
        }

        /// <summary>
        /// Member representation.
        /// </summary>
        /// <param name="import">The import.</param>
        [ParameterTypes(typeof(Lazy<string>))]
        public void _set_Import(Lazy<InstanceWrap> import)
        {
            Import.Set(import);
        }

        /// <summary>
        /// Member representation.
        /// </summary>
        /// <returns>Lazy&lt;InstanceWrap&gt;.</returns>
        [ReturnType(typeof(Lazy<string>))]
        public Lazy<InstanceWrap> _get_Import()
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