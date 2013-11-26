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

            MethodOperand = getMethodInfo(Data as MethodInfo);
            BranchOperandAddress = getBranchOffset(instruction);
        }

        public CILInstruction(Instruction instruction)
        {
            Address = instruction.Offset;
            Data = instruction.Operand;

            OpCode = instruction.OpCode;
            MethodOperand = CreateMethodInfo(Data as MethodReference);
            BranchOperandAddress = getBranchOffset(Data as Instruction);
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

        private int getBranchOffset(Instruction instruction)
        {
            if (instruction == null)
                return -1;

            return instruction.Offset;
        }

        internal static InstanceInfo GetInfo(TypeReference type)
        {
            return new InstanceInfo(type.FullName);
        }

        internal static TypeMethodInfo CreateMethodInfo(MethodReference method)
        {
            if (method == null)
                return null;

            var paramInfos = new List<ParameterTypeInfo>();
            foreach (var param in method.Parameters)
            {
                var paramInfo = ParameterTypeInfo.Create(param.Name, GetInfo(param.ParameterType));
                paramInfos.Add(paramInfo);
            }

            return new TypeMethodInfo(
                   GetInfo(method.DeclaringType),
                   method.Name,
                   GetInfo(method.ReturnType),
                   paramInfos.ToArray(),
                   true, //TODO
                   method.HasGenericParameters,
                   false //TODO
                   );
        }
    }
}
