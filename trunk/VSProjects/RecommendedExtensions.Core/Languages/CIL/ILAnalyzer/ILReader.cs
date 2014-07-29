using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection.Emit;
using System.Reflection;

namespace RecommendedExtensions.Core.Languages.CIL.ILAnalyzer
{
    /// <summary>
    /// Taken from answer at: http://stackoverflow.com/questions/14243284/how-can-i-retrieve-string-literals-using-reflection
    /// Reads IL instructions from a byte stream.
    /// </summary>
    /// <remarks>Allows generated code to be viewed without debugger or enabled debug assemblies.</remarks>
    sealed class ILReader
    {
        /// <summary>
        /// The _instruction lookup.
        /// </summary>
        private static readonly Dictionary<short, OpCode> instructionLookup = ILReader.GetLookupTable();

        /// <summary>
        /// All opcodes available in lookup table.
        /// </summary>
        /// <value>The opcodes.</value>
        internal static IEnumerable<OpCode> Opcodes { get { return instructionLookup.Values; } }

        /// <summary>
        /// The IL reader provider.
        /// </summary>
        private IILReaderProvider intermediateLanguageProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ILReader" /> class.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <exception cref="System.ArgumentNullException">method</exception>
        public ILReader(MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            this.intermediateLanguageProvider = ILReader.CreateILReaderProvider(method);
        }

        /// <summary>
        /// Gets the instructions.
        /// </summary>
        /// <value>The instructions.</value>
        /// <exception cref="System.InvalidProgramException"></exception>
        public IEnumerable<ILInstruction> Instructions
        {
            get
            {
                byte[] instructionBytes = this.intermediateLanguageProvider.GetMethodBody();
                int instructionIndex = 0, startAddress;
                for (int position = 0; position < instructionBytes.Length; )
                {
                    startAddress = position;
                    short operationData = instructionBytes[position];
                    if (IsInstructionPrefix(operationData))
                    {
                        operationData = (short)((operationData << 8) | instructionBytes[++position]);
                    }

                    position++;

                    OpCode code;
                    if (!instructionLookup.TryGetValue(operationData, out code))
                    {
                        throw new InvalidProgramException(string.Format("0x{0:X2} is not a valid op code.", operationData));
                    }

                    int dataSize = GetSize(code.OperandType);
                    byte[] data = new byte[dataSize];
                    Buffer.BlockCopy(instructionBytes, position, data, 0, dataSize);
                    object objData = this.GetData(code, data);
                    position += dataSize;

                    if (code.OperandType == OperandType.InlineSwitch)
                    {
                        dataSize = (int)objData;
                        int[] labels = new int[dataSize];
                        for (int index = 0; index < labels.Length; index++)
                        {
                            labels[index] = BitConverter.ToInt32(instructionBytes, position);
                            position += 4;
                        }

                        objData = labels;
                    }

                    yield return new ILInstruction(code, data, startAddress, objData, instructionIndex);
                    instructionIndex++;
                }
            }
        }


        /// <summary>
        /// Creates the IL reader provider.
        /// </summary>
        /// <param name="methodInfo">The MethodInfo object that represents the method to read..</param>
        /// <returns>The ILReader provider.</returns>
        private static IILReaderProvider CreateILReaderProvider(MethodInfo methodInfo)
        {
            IILReaderProvider reader = DynamicILReaderProvider.Create(methodInfo);
            if (reader != null)
            {
                return reader;
            }

            return new ILReaderProvider(methodInfo);
        }

        /// <summary>
        /// Checks to see if the IL instruction is a prefix indicating the length of the instruction is two bytes long.
        /// </summary>
        /// <param name="value">The IL instruction as a byte.</param>
        /// <returns>True if this IL instruction is a prefix indicating the instruction is two bytes long.</returns>
        /// <remarks>IL instructions can either be 1 or 2 bytes.</remarks>
        private static bool IsInstructionPrefix(short value)
        {
            return ((value & OpCodes.Prefix1.Value) == OpCodes.Prefix1.Value) || ((value & OpCodes.Prefix2.Value) == OpCodes.Prefix2.Value)
                        || ((value & OpCodes.Prefix3.Value) == OpCodes.Prefix3.Value) || ((value & OpCodes.Prefix4.Value) == OpCodes.Prefix4.Value)
                        || ((value & OpCodes.Prefix5.Value) == OpCodes.Prefix5.Value) || ((value & OpCodes.Prefix6.Value) == OpCodes.Prefix6.Value)
                        || ((value & OpCodes.Prefix7.Value) == OpCodes.Prefix7.Value) || ((value & OpCodes.Prefixref.Value) == OpCodes.Prefixref.Value);
        }

        /// <summary>
        /// The get lookup table.
        /// </summary>
        /// <returns>A dictionary of IL instructions.</returns>
        private static Dictionary<short, OpCode> GetLookupTable()
        {
            // Might be better to do an array lookup.  Use a seperate arrary for instructions without a prefix and array for each prefix.
            Dictionary<short, OpCode> lookupTable = new Dictionary<short, OpCode>();
            FieldInfo[] fields = typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                OpCode code = (OpCode)field.GetValue(null);
                lookupTable.Add(code.Value, code);
            }

            return lookupTable;
        }

        /// <summary>
        /// Gets the size of a operand.
        /// </summary>
        /// <param name="operandType">Defines the type of operand.</param>
        /// <returns>The size in bytes of the operand type.</returns>
        private static int GetSize(OperandType operandType)
        {
            switch (operandType)
            {
                case OperandType.InlineNone:
                    return 0;
                case OperandType.ShortInlineBrTarget:
                case OperandType.ShortInlineI:
                case OperandType.ShortInlineVar:
                    return 1;
                case OperandType.InlineVar:
                    return 2;
                case OperandType.InlineBrTarget:
                case OperandType.InlineField:
                case OperandType.InlineI:
                case OperandType.InlineMethod:
                case OperandType.InlineSig:
                case OperandType.InlineString:
                case OperandType.InlineSwitch:
                case OperandType.InlineTok:
                case OperandType.InlineType:
                case OperandType.ShortInlineR:
                    return 4;
                case OperandType.InlineI8:
                case OperandType.InlineR:
                    return 8;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="rawData">The raw data.</param>
        /// <returns>System.Object.</returns>
        private object GetData(OpCode code, byte[] rawData)
        {
            object data = null;
            switch (code.OperandType)
            {
                case OperandType.InlineField:
                    data = this.intermediateLanguageProvider.ResolveField(BitConverter.ToInt32(rawData, 0));
                    break;
                case OperandType.InlineSwitch:
                    data = BitConverter.ToInt32(rawData, 0);
                    break;
                case OperandType.InlineBrTarget:
                case OperandType.InlineI:
                    data = BitConverter.ToInt32(rawData, 0);
                    break;
                case OperandType.InlineI8:
                    data = BitConverter.ToInt64(rawData, 0);
                    break;
                case OperandType.InlineMethod:
                    data = this.intermediateLanguageProvider.ResolveMethod(BitConverter.ToInt32(rawData, 0));
                    break;
                case OperandType.InlineR:
                    data = BitConverter.ToDouble(rawData, 0);
                    break;
                case OperandType.InlineSig:
                    data = this.intermediateLanguageProvider.ResolveSignature(BitConverter.ToInt32(rawData, 0));
                    break;
                case OperandType.InlineString:
                    data = this.intermediateLanguageProvider.ResolveString(BitConverter.ToInt32(rawData, 0));
                    break;
                case OperandType.InlineTok:
                case OperandType.InlineType:
                    data = this.intermediateLanguageProvider.ResolveType(BitConverter.ToInt32(rawData, 0));
                    break;
                case OperandType.InlineVar:
                    data = BitConverter.ToInt16(rawData, 0);
                    break;
                case OperandType.ShortInlineVar:
                case OperandType.ShortInlineI:
                    data = (sbyte)rawData[0];
                    break;
                case OperandType.ShortInlineBrTarget:
                    data = (int)(sbyte)rawData[0];
                    break;
                case OperandType.ShortInlineR:
                    data = BitConverter.ToSingle(rawData, 0);
                    break;
            }

            return data;
        }
    }

}
