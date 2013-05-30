using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection.Emit;
namespace TypeSystem.TypeBuilding
{
    class StringModifyWriter:ILInstructionWriter
    {
        public StringModifyWriter(ILGenerator generator):base(generator)
        {
        }
        protected override void Emit(System.Reflection.Emit.OpCode opcode, string data)
        {
            base.Emit(opcode, "Modified: "+data);
        }
        protected override void Emit(OpCode opcode, System.Reflection.FieldInfo field)
        {
            if (field.FieldType == typeof(string))
            {
                base.Emit(OpCodes.Ldstr, "Modified string field" + field);
                return;
            }

            base.Emit(opcode, field);
        }
    }
}
