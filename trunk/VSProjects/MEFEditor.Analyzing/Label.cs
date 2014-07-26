using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing
{
    public class Label
    {
        public readonly string LabelName;
        public uint InstructionOffset { get; private set; }
        internal Label(string labelName)
        {
            LabelName = labelName;            
        }

        internal void SetOffset(uint instructionOffset)
        {
            InstructionOffset = instructionOffset;
        }

        public override string ToString()
        {
            return "[Label]" + LabelName;
        } 
    }
}
