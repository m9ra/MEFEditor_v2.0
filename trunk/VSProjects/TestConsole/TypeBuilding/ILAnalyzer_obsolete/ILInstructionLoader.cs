using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;


namespace TypeSystem.TypeBuilding.ILAnalyzer
{
    public static class ILInstructionLoader
    {
        public static IEnumerable<ILInstruction> GetInstructions(MethodBase methodBase)
        {
            var bytes = ILUtilities.GetILBytesDynamic(methodBase);

            int offset = 0;

            while (offset < bytes.Length)
            {
                ILInstruction instruction = new ILInstruction();
                instruction.Offset = offset;

                short code = (short)bytes[offset++];
                if (code == 0xfe)
                {
                    code = (short)(bytes[offset++] | 0xfe00);
                }

                instruction.OpCode = ILOpCodeTranslator.GetOpCode(code);
                //!!!contains valid metadatatoken only if it has meaning for instruction
                int metaDataToken = bytes.GetInt32Safe(offset);

                switch (instruction.OpCode.OperandType)
                {
                    case OperandType.InlineBrTarget:
                        offset += 4;
                        break;

                    case OperandType.InlineField:
                        offset += 4;
                        break;

                    case OperandType.InlineI:
                        instruction.Data = bytes.GetInt32Safe(offset);
                        offset += 4;
                        break;

                    case OperandType.InlineI8:
                        instruction.Data = bytes.GetInt64(offset);
                        offset += 8;
                        break;

                    case OperandType.InlineMethod:
                        

                        Type[] genericMethodArguments = null;
                        if (methodBase.IsGenericMethod == true)
                        {
                            genericMethodArguments = methodBase.GetGenericArguments();
                        }

                        Type[] genericArgs=null;
                        if (methodBase.DeclaringType != null)
                        {
                            genericArgs = methodBase.DeclaringType.GetGenericArguments();
                        }


                        instruction.Data = methodBase.Module.ResolveMethod(metaDataToken, genericArgs, genericMethodArguments);
                        offset += 4;
                        break;

                    case OperandType.InlineNone:
                        break;

                    case OperandType.InlineR:
                        offset += 8;
                        break;

                    case OperandType.InlineSig:
                        offset += 4;
                        break;

                    case OperandType.InlineString:                        
                        instruction.Data = methodBase.Module.ResolveString(metaDataToken);     
                     
                        offset += 4;
                        
                        break;

                    case OperandType.InlineSwitch:
                        int count = bytes.GetInt32Safe(offset) + 1;
                        offset += 4 * count;
                        break;

                    case OperandType.InlineTok:
                        offset += 4;
                        break;

                    case OperandType.InlineType:
                        offset += 4;
                        break;

                    case OperandType.InlineVar:
                        offset += 2;
                        break;

                    case OperandType.ShortInlineBrTarget:
                        instruction.Data = (byte)bytes[offset];
                        offset += 1;
                        break;

                    case OperandType.ShortInlineI:
                        offset += 1;
                        break;

                    case OperandType.ShortInlineR:
                        offset += 4;
                        break;

                    case OperandType.ShortInlineVar:
                        offset += 1;
                        break;

                    default:
                        throw new NotImplementedException();
                }

                yield return instruction;
            }
        }
    }
}
