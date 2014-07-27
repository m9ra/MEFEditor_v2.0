using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using MEFEditor.TypeSystem;
using MEFEditor.TypeSystem.Runtime;

namespace MEFEditor.UnitTesting.RuntimeTypeDefinitions
{
    public class SimpleType : DataTypeDefinition
    {
        protected Field<string> TestProperty;

        public SimpleType()
        {
            FullName = "SimpleType";
        }

        public void _method_ctor(string data)
        {
            TestProperty.Set(data);
        }

        public string _method_Concat(string concated = "CallDefault")
        {
            return TestProperty.Get() + "_" + concated;
        }

        protected override void draw(InstanceDrawer services)
        {
            services.PublishField("TestProperty", TestProperty);
            services.ForceShow();

        }
    }
}
