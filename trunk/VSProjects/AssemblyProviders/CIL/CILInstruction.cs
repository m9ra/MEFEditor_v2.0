using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using E = System.Reflection.Emit;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Analyzing;
using TypeSystem;
using TypeSystem.TypeParsing;

using AssemblyProviders.CIL.ILAnalyzer;

namespace AssemblyProviders.CIL
{
    public class CILInstruction
    {
        private static readonly Dictionary<string, OpCode> OpCodesTable = new Dictionary<string, OpCode>(StringComparer.OrdinalIgnoreCase);

        public readonly int Address;

        public readonly object Data;

        public readonly OpCode OpCode;

        public readonly TypeMethodInfo MethodOperand;

        public readonly int BranchOperandAddress = -1;

        public readonly TypeMethodInfo Setter;

        public readonly TypeMethodInfo Getter;

        static CILInstruction()
        {
            foreach (var opCodeField in typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                var opCode = (OpCode)opCodeField.GetValue(null);
                OpCodesTable.Add(opCode.Name, opCode); ;
            }
        }


        public CILInstruction(ILInstruction instruction)
        {
            Address = instruction.Address;
            Data = instruction.Data;

            OpCode = OpCodesTable[instruction.OpCode.Name];

            //TODO resolve ctors
            MethodOperand = getMethodInfo(Data as MethodInfo);
            BranchOperandAddress = getBranchOffset(instruction);
            Setter = getSetter(Data as FieldInfo);
            Getter = getGetter(Data as FieldInfo);
        }

        public CILInstruction(Instruction instruction)
        {
            Address = instruction.Offset;
            Data = instruction.Operand;

            OpCode = instruction.OpCode;
            MethodOperand = CreateMethodInfo(Data as MethodReference, needsDynamicResolution(OpCode));
            BranchOperandAddress = getBranchOffset(Data as Instruction);

            Setter = getSetter(Data as FieldReference);
            Getter = getGetter(Data as FieldReference);
        }


        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("0x{0:x4} {1,-10}", this.Address, this.OpCode.Name);

            if (this.Data != null)
            {
                if (this.Data is string)
                {
                    builder.Append("\"" + this.Data + "\"");
                }
                else
                {
                    builder.Append(this.Data.ToString());
                }
            }

            return builder.ToString();
        }


        #region Reflection instructions

        private TypeMethodInfo getGetter(FieldInfo field)
        {
            if (field == null)
                return null;

            var name = "get_" + field.Name;
            var declaringType = TypeDescriptor.Create(field.DeclaringType);
            var fieldType = TypeDescriptor.Create(field.FieldType);

            //TODO resolve if it is static
            return new TypeMethodInfo(declaringType,
                name, fieldType, ParameterTypeInfo.NoParams,
                true, TypeDescriptor.NoDescriptors);
        }

        private TypeMethodInfo getSetter(FieldInfo field)
        {
            if (field == null)
                return null;

            var name = "set_" + field.Name;
            var declaringType = TypeDescriptor.Create(field.DeclaringType);
            var fieldType = TypeDescriptor.Create(field.FieldType);

            //TODO resolve if it is static
            return new TypeMethodInfo(declaringType,
                name, TypeDescriptor.Void, new ParameterTypeInfo[]{
                    ParameterTypeInfo.Create("value",fieldType)
                },
                true, TypeDescriptor.NoDescriptors);
        }

        private int getBranchOffset(ILInstruction instruction)
        {
            switch (instruction.OpCode.OperandType)
            {
                case E.OperandType.ShortInlineBrTarget:
                    return instruction.Address + instruction.Length + (int)Data;
                case E.OperandType.InlineBrTarget:
                    throw new NotImplementedException();
                default:
                    return -1;
            }
        }

        private TypeMethodInfo getMethodInfo(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                return null;

            return TypeMethodInfo.Create(methodInfo);
        }

        #endregion


        #region Mono Cecil instructions

        private bool needsDynamicResolution(OpCode opcode)
        {
            switch (opcode.Name) { 
            
                case "callvirt":
                    return true;
                default:
                    return false;   
            }
        }

        private TypeMethodInfo getGetter(FieldReference field)
        {
            if (field == null)
                return null;

            var name = "get_" + field.Name;
            var declaringType = GetInfo(field.DeclaringType);
            var fieldType = GetInfo(field.FieldType);

            //TODO resolve if it is static
            return new TypeMethodInfo(declaringType,
                name, fieldType, ParameterTypeInfo.NoParams,
                true, TypeDescriptor.NoDescriptors);
        }

        private TypeMethodInfo getSetter(FieldReference field)
        {
            if (field == null)
                return null;

            var name = "set_" + field.Name;
            var declaringType = GetInfo(field.DeclaringType);
            var fieldType = GetInfo(field.FieldType);

            //TODO resolve if it is static
            return new TypeMethodInfo(declaringType,
                name, TypeDescriptor.Void, new ParameterTypeInfo[]{
                    ParameterTypeInfo.Create("value",fieldType)
                },
                true, TypeDescriptor.NoDescriptors);
        }

        private int getBranchOffset(Instruction instruction)
        {
            if (instruction == null)
                return -1;

            return instruction.Offset;
        }

        internal static TypeDescriptor GetInfo(TypeReference type)
        {
            return TypeDescriptor.Create(type.FullName);
        }

        internal static TypeMethodInfo CreateMethodInfo(MethodReference method, bool needsDynamicResolution)
        {
            if (method == null)
                return null;

            var builder = new MethodInfoBuilder(method);
            builder.NeedsDynamicResolving = needsDynamicResolution;

            return builder.Build();
        }

        #endregion
    }
}
