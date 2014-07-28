﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Drawing;
using MEFEditor.Analyzing;
using MEFEditor.TypeSystem;
using MEFEditor.TypeSystem.Runtime;

namespace MEFEditor.UnitTesting.RuntimeTypeDefinitions
{
    public class SelfStringExport : DataTypeDefinition
    {
        protected Field<string> Export;

        public SelfStringExport()
        {
            FullName = "SelfStringExport";
            var builder = new ComponentInfoBuilder(GetTypeInfo());
            builder.AddSelfExport(false,typeof(string).FullName);
            builder.SetImportingCtor(TypeDescriptor.Create<string>());
            ComponentInfo = builder.BuildWithImplicitCtor();
        }

        public void _method_ctor(string toExport)
        {

        }
    }
}
