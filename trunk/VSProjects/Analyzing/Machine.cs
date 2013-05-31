using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing
{
    /// <summary>
    /// Virtual machine that provides analyzing services
    /// NOTE: Is not thread safe
    /// </summary>
    public class Machine
    {
        /// <summary>
        /// Run analysis of program loaded via given loader. Execution starts from loader.EntryPoint
        /// </summary>
        /// <param name="loader">Loader which provides instrution generation and type/methods resolving</param>
        /// <returns>Result of analysis</returns>
        public AnalyzingResult Run(IInstructionLoader loader)
        {
            var context = new Execution.AnalyzingContext();
            context.FetchCallInstructions(loader.EntryPoint);

            //TODO caching services via wrapped loader, ...
            while (!context.IsExecutionEnd)
            {
                var instruction = context.NextInstruction();
                if (instruction == null)
                {
                    break;
                }

                instruction.Execute(context);
            }

            return context.GetResult();
        }
    }
}
