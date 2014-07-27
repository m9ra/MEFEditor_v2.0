using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MEFEditor.TypeSystem;
using MEFEditor.TypeSystem.Runtime;

namespace MEFEditor.UnitTesting.RuntimeTypeDefinitions
{
    public class MetaInterface : DataTypeDefinition
    {
        public MetaInterface()
        {
            FullName = "MetaInterface";
        }

        public string[] _get_Key1()
        {
            return new[] { "Interface method cannot be called" };
        }
    }
}
