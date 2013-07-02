﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution;

namespace Analyzing
{
    /// <summary>
    /// Virtual machine that provides analyzing services
    /// NOTE: Is not thread safe
    /// </summary>
    public class Machine
    {
        AnalyzingInstructionLoader _cachedLoader;
        
        public Machine(MachineSettings settings)
        {
            _cachedLoader = new AnalyzingInstructionLoader(settings);
        }




        /// <summary>
        /// Run analysis of program loaded via given loader. Execution starts from loader.EntryPoint
        /// </summary>
        /// <param name="loader">Loader which provides instrution generation and type/methods resolving</param>
        /// <returns>Result of analysis</returns>
        public AnalyzingResult Run(IInstructionLoader loader)
        {
            _cachedLoader.SetLoader(loader);

            return run();
        }

        /// <summary>
        /// Run instructions present in _cachedLoader
        /// </summary>
        /// <returns>Result of analysis</returns>
        private AnalyzingResult run()
        {
            var context = new Execution.AnalyzingContext(_cachedLoader);
            context.PrepareCall(new VariableName[] { }); //no input arguments
            context.FetchCallInstructions(_cachedLoader.EntryPoint);

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
