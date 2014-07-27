using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing.Editing;
using MEFEditor.Analyzing.Execution.Instructions;

namespace MEFEditor.Analyzing
{
    public class AssignBuilder
    {
        readonly AssignBase _instruction;
        internal AssignBuilder(AssignBase instruction)
        {
            _instruction = instruction;
        }

        public RemoveTransformProvider RemoveProvider
        {
            set
            {
                _instruction.RemoveProvider = value;
            }
        }
    }
}
