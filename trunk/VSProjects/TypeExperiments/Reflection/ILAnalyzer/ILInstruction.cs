using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;
using System.Reflection.Emit;
using System.Runtime.InteropServices;


namespace TypeExperiments.Reflection.ILAnalyzer
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ILInstruction
    {
        public readonly OpCode OpCode; // 40.  56-64.  The entire structure is very big.  maybe do array lookup for opcode instead.

        public readonly byte[] RawData;

        public readonly object Data;

        public readonly int Address;

        public readonly int Index;

        internal ILInstruction(OpCode code, byte[] instructionRawData, int instructionAddress, object instructionData, int index)
        {
            this.OpCode = code;
            this.RawData = instructionRawData;
            this.Address = instructionAddress;
            this.Data = instructionData;
            this.Index = index;
        }



        /// <summary>
        /// Gets the value as integer
        /// </summary>
        /// <value>The data value.</value>
        public int DataValue
        {
            get
            {
                int value = 0;
                if (this.Data != null)
                {
                    if (this.Data is byte)
                    {
                        value = (byte)this.Data;
                    }
                    else if (this.Data is short)
                    {
                        value = (short)this.Data;
                    }
                    else if (this.Data is int)
                    {
                        value = (int)this.Data;
                    }
                }

                return value;
            }
        }

        /// <summary>
        /// Gets the length of the instructions and operands.
        /// </summary>
        /// <value>The length.</value>
        public int Length
        {
            get
            {
                return this.OpCode.Size + (this.RawData == null ? 0 : this.RawData.Length);
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
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

            if (this.RawData != null && this.RawData.Length > 0)
            {
                builder.Append(" [0x");
                for (int i = this.RawData.Length - 1; i >= 0; i--)
                {
                    builder.Append(this.RawData[i].ToString("x2", CultureInfo.InvariantCulture));
                }

                builder.Append(']');
            }

            return builder.ToString();
        }
    }
}
