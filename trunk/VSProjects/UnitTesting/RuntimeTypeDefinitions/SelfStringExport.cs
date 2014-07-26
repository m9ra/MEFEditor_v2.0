using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Drawing;
using Analyzing;
using TypeSystem;
using TypeSystem.Runtime;

namespace UnitTesting.RuntimeTypeDefinitions
{
    public class SelfStringExport : DataTypeDefinition
    {
        protected Field<string> Export;

        public SelfStringExport()
        {
            FullName = "SelfStringExport";
            var builder = new ComponentInfoBuilder(GetTypeInfo());
            builder.AddSelfExport(typeof(string).FullName);
            builder.SetImportingCtor(TypeDescriptor.Create<string>());
            ComponentInfo = builder.BuildWithImplicitCtor();
        }

        public void _method_ctor(string toExport)
        {

        }
    }
}
