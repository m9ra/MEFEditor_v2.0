using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution;
using Analyzing.Execution.Instructions;

namespace Analyzing
{
    /// <summary>
    /// Represents Instructions emitted by Emitter. 
    /// </summary>
    public class InstructionBatch
    {
        /// <summary>
        /// Emitted instructions representing program to be runned
        /// </summary>
        internal readonly InstructionBase[] Instructions;

        /// <summary>
        /// Create instruction batch from given instructions
        /// </summary>
        /// <param name="instructions">Instructions that will be present in batch</param>
        internal InstructionBatch(InstructionBase[] instructions)
        {
            Instructions = instructions;
        }

        /// <summary>
        /// String representation of contained instructions
        /// </summary>
        public string Code
        {
            get
            {
                var result = new StringBuilder();
                InstructionInfo currentInfo = null;

                var line = 0;
                foreach (var instruction in Instructions)
                {
                    if (instruction.Info != currentInfo)
                    {
                        currentInfo = instruction.Info;
                        if (currentInfo.Comment != null)
                            result.AppendLine(currentInfo.Comment);
                    }

                    var pointingLabel = getLabelToLine(line);
                    if (pointingLabel != null)
                    {
                        result.AppendLine(pointingLabel.ToString());
                    }

                    result.AppendLine(instruction.ToString());
                    ++line;
                }

                return result.ToString();
            }
        }

        /// <summary>
        /// Get Label contained in jump instructions pointing to given line
        /// </summary>
        /// <param name="line">Line where label has to point</param>
        /// <returns>Label pointing to given line or null if there is no such line</returns>
        private Label getLabelToLine(int line)
        {
            foreach (var instr in Instructions)
            {
                var jmp = instr as Jump;
                var jmpif = instr as ConditionalJump;

                Label label = null;
                if (jmp != null)
                    label = jmp.Target;
                if (jmpif != null)
                    label = jmpif.Target;

                if (label == null)
                    continue;

                if (label.InstructionOffset == line)
                    return label;
            }
            return null;
        }
    }
}
