using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeSystem.Runtime;

namespace UnitTesting.RuntimeTypeDefinitions
{
    class SimpleType:RuntimeTypeDefinition
    {
        public readonly Field<string> TestProperty;

        public SimpleType()
        {
            TestProperty = new Field<string>(this);
            FullName = "SimpleType";
        }

        public void _method_ctor(string data)
        {
            TestProperty.Set(data);
        }

        public string _method_Concat(string concated)
        {
            return TestProperty.Get()+"_"+concated;
        }
    }
}
