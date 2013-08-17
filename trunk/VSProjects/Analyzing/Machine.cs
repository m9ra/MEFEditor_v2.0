using System;
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
    public class Machine<MethodID,InstanceInfo>
    {
        LoaderBase<MethodID,InstanceInfo> _loader;
        IMachineSettings<InstanceInfo> _settings;

        public Machine(IMachineSettings<InstanceInfo> settings)
        {
            _settings = settings;    
        }


        /// <summary>
        /// Run analysis of program loaded via given loader. Execution starts from loader.EntryPoint
        /// </summary>
        /// <param name="loader">Loader which provides instrution generation and type/methods resolving</param>
        /// <returns>Result of analysis</returns>
        public AnalyzingResult<MethodID, InstanceInfo> Run(LoaderBase<MethodID, InstanceInfo> loader)
        {
            _loader = loader;

            return run();
        }

        /// <summary>
        /// Run instructions present in _cachedLoader
        /// </summary>
        /// <returns>Result of analysis</returns>
        private AnalyzingResult<MethodID, InstanceInfo> run()
        {
            var context = new Execution.AnalyzingContext<MethodID, InstanceInfo>(_settings,_loader);
            context.PrepareCall(new VariableName[] { }); //no input arguments
            context.FetchCallInstructions(new VersionedName("EntryPoint",0),_loader.EntryPoint);

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
