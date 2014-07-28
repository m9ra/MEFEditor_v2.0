using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing
{
    /// <summary>
    /// Named label that specify target of jump for <see cref="Machine"/>.
    /// </summary>
    public class Label
    {
        /// <summary>
        /// The label name.
        /// </summary>
        public readonly string LabelName;

        /// <summary>
        /// Gets the instruction offset.
        /// </summary>
        /// <value>The instruction offset.</value>
        public uint InstructionOffset { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Label"/> class.
        /// </summary>
        /// <param name="labelName">Name of the label.</param>
        internal Label(string labelName)
        {
            LabelName = labelName;
        }

        /// <summary>
        /// Sets the offset of target instruction for jump instruction.
        /// </summary>
        /// <param name="instructionOffset">The target instruction offset.</param>
        internal void SetOffset(uint instructionOffset)
        {
            InstructionOffset = instructionOffset;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return "[Label]" + LabelName;
        }
    }
}
