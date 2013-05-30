using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution
{
    interface IInstruction
    {
        void Execute(Context context);
    }
}
