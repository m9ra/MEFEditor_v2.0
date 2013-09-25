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
    public class Machine
    {
        internal readonly IMachineSettings Settings;

        public Machine(IMachineSettings settings)
        {
            Settings = settings;
        }

        public Instance CreateDirectInstance(object directObject, InstanceInfo info = null)
        {
            if (info == null)
            {
                info = Settings.GetNativeInfo(directObject.GetType());
            }

            if (!Settings.IsDirect(info))
                throw new NotSupportedException("Cannot create direct instance from not direct info");

            var instance = new DirectInstance(directObject, info, this);
            Settings.InstanceCreated(instance);

            return instance;
        }

        internal Instance CreateInstance(InstanceInfo info)
        {
            if (Settings.IsDirect(info))
            {
                return CreateDirectInstance(null, info);
            }

            var instance = new DataInstance(info);
            Settings.InstanceCreated(instance);

            return instance;
        }

        /// <summary>
        /// Run analysis of program loaded via given loader. Execution starts from loader.EntryPoint
        /// </summary>
        /// <param name="loader">Loader which provides instrution generation and type/methods resolving</param>
        /// <returns>Result of analysis</returns>
        public AnalyzingResult Run(LoaderBase loader, params Instance[] arguments)
        {
            return run(loader, arguments);
        }

        /// <summary>
        /// Run instructions present in _cachedLoader
        /// </summary>
        /// <returns>Result of analysis</returns>
        private AnalyzingResult run(LoaderBase loader, params Instance[] arguments)
        {
            var context = new Execution.AnalyzingContext(this, loader);
            context.DynamicCall("EntryPoint", loader.EntryPoint, arguments);

            while (!context.IsExecutionEnd)
            {
                var instruction = context.NextInstruction();
                if (instruction == null)
                {
                    break;
                }

                context.Prepare(instruction);
                instruction.Execute(context);
            }

            return context.GetResult();
        }
    }
}
