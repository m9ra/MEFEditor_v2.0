using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Reflection.Emit;

using TypeExperiments.Reflection.ILAnalyzer;

namespace TypeExperiments.TypeBuilding
{
    class ILInstructionWriter
    {
        /// <summary>
        /// Target for writing generated IL code
        /// </summary>
        ILGenerator _target;

        

        public ILInstructionWriter(ILGenerator targetGenerator)
        {
            _target = targetGenerator;
        }

        protected virtual void Emit(OpCode opcode)
        {
            _target.Emit(opcode);
        }

        protected virtual void Emit(OpCode opcode, MethodInfo method)
        {
            _target.Emit(opcode, method);
        }

        protected virtual void Emit(OpCode opcode, string data)
        {
            _target.Emit(opcode, data);
        }

        protected virtual void Emit(OpCode opcode, int data)
        {
            _target.Emit(opcode, data);
        }

        protected virtual void Emit(OpCode opcode, FieldInfo field)
        {
            _target.Emit(opcode, field);
        }

        protected virtual void Emit(OpCode opcode, byte shortOffset)
        {
            _target.Emit(opcode, shortOffset);
        }

        /// <summary>
        /// Write instruction into given generator
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="generator"></param>
        public void Write(ILInstruction instruction)
        {
            var opcode = instruction.OpCode;
            var data = instruction.Data;

            switch (opcode.OperandType)
            {
                case OperandType.InlineMethod:
                    Emit(opcode, (MethodInfo)data);
                    break;
                case OperandType.InlineNone:
                    Emit(opcode);
                    break;
                case OperandType.InlineString:
                    Emit(opcode, (string)data);
                    break;
                case OperandType.InlineI:
                    Emit(opcode, (int)data);
                    break;
                case OperandType.ShortInlineBrTarget:
                    Emit(opcode, (byte)data);
                    break;
                case OperandType.InlineField:
                    Emit(opcode, (FieldInfo)data);
                    break;
                default:
                    throw new NotSupportedException("given opcode is not supported");
            }
        }

        public static void WriteIL(IEnumerable<ILInstruction> instructions, ILGenerator generator)
        {
            var writer = new ILInstructionWriter(generator);
            foreach (var instruction in instructions)
            {
                writer.Write(instruction);
            }
        }

    }
}
