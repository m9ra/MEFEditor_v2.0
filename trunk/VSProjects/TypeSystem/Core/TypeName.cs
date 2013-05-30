using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeSystem.Core
{
    class TypeName:GenericStrongName
    {
        public TypeName(string name):base(name)
        {
        }

        public static TypeName From(Type type)
        {
            return new TypeName(type.FullName);
        }     
    }
}
