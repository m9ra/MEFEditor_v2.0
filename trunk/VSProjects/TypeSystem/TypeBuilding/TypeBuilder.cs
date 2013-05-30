using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeSystem.Core;
using TypeSystem.Reflection;
using TypeSystem.Reflection.Definitions;

namespace TypeSystem.TypeBuilding
{
    internal class InternalTypeBuilder
    {
        public readonly TypeName Name;
        public bool IsBuilded { get; private set; }

        private InternalType buildedType;

        public InternalTypeBuilder(TypeName name,TypeName baseName){
            this.Name = name;
        }

        public void Add(FieldDefinition field)
        {
            throwIfBuilded();
            throw new NotImplementedException("Add field");
        }

        public void Add(MethodDefinition method)
        {
        }

        private void throwIfBuilded()
        {
            if (IsBuilded)
                throw new NotSupportedException("Type has alread been builded");
        }



        internal InternalType CreateType()
        {
            throw new NotImplementedException();
        }
    }
}
