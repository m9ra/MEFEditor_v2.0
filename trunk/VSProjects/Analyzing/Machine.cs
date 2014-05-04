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
        /// <summary>
        /// All instances that were created during last execution
        /// </summary>
        private readonly Dictionary<string, Instance> _createdInstances = new Dictionary<string, Instance>();

        /// <summary>
        /// Settings available for virtual machine
        /// </summary>
        internal readonly MachineSettingsBase Settings;

        /// <summary>
        /// Initialize machine with specified settings
        /// </summary>
        /// <param name="settings">Settings specified for machine</param>
        public Machine(MachineSettingsBase settings)
        {
            Settings = settings;
        }

        /// <summary>
        /// Create direct instance from given object. Only supported direct instances
        /// can be created. If directObject has unsupported type, exception is throwed.
        /// </summary>
        /// <param name="directObject">Direct object used for instance creation</param>
        /// <param name="info">Instance info used for creating. If null, default info is taken from direct object</param>
        /// <returns>Created instance</returns>
        public Instance CreateDirectInstance(object directObject, InstanceInfo info = null)
        {
            if (info == null)
            {
                info = Settings.GetNativeInfo(directObject.GetType());
            }

            if (!Settings.IsDirect(info))
                throw new NotSupportedException("Cannot create direct instance from not direct info");

            var instance = new DirectInstance(directObject, info, this);
            registerInstance(instance);

            return instance;
        }

        /// <summary>
        /// Creates instance for given info. If info belongs to direct instance,
        /// direct instance will be created from default value.
        /// </summary>
        /// <param name="info">Info determining type of created instance</param>
        /// <returns>Created instance</returns>
        public Instance CreateInstance(InstanceInfo info)
        {
            if (Settings.IsDirect(info))
            {
                return CreateDirectInstance(null, info);
            }

            var instance = new DataInstance(info);
            registerInstance(instance);

            return instance;
        }

        /// <summary>
        /// 
        /// TODO accept entry method ID
        /// 
        /// Run analysis of program loaded via given loader. Execution starts from loader.EntryPoint
        /// </summary>
        /// <param name="loader">Loader which provides instrution generation and type/methods resolving</param>
        /// <returns>Result of analysis</returns>
        public AnalyzingResult Run(LoaderBase loader, MethodID entryMethod, params Instance[] arguments)
        {
            return run(loader, entryMethod, arguments);
        }

        internal string CreateID(string hint)
        {
            var currentID = hint;
            var number = 0;
            while (_createdInstances.ContainsKey(currentID))
            {
                currentID = string.Format("{0}_{1}", hint, number);
                ++number;
            }

            return currentID;
        }

        internal void ReportIDChange(string oldID)
        {
            var instance = _createdInstances[oldID];
            _createdInstances.Add(instance.ID, instance);
        }

        /// <summary>
        /// Run instructions present in _cachedLoader
        /// </summary>
        /// <returns>Result of analysis</returns>
        private AnalyzingResult run(LoaderBase loader, MethodID entryMethod, params Instance[] arguments)
        {
            Settings.FireBeforeInterpretation();
            try
            {
                _createdInstances.Clear();
                var context = new Execution.AnalyzingContext(this, loader);

                foreach (var argument in arguments)
                {
                    _createdInstances.Add(argument.ID, argument);
                }
                context.FetchCall(entryMethod, arguments);

                var executionLimit = Settings.ExecutionLimit;

                //interpretation processing
                while (!context.IsExecutionEnd)
                {
                    //limit execution
                    --executionLimit;
                    if (executionLimit < 0)
                        throw new NotImplementedException("Execution limit has been reached");

                    //process instruction
                    var instruction = context.NextInstruction();
                    if (instruction == null)
                    {
                        break;
                    }

                    context.Prepare(instruction);
                    instruction.Execute(context);
                }

                var result = context.GetResult(new Dictionary<string, Instance>(_createdInstances));
                return result;
            }
            finally
            {
                Settings.FireAfterInterpretation();
            }
        }

        /// <summary>
        /// All instances are registered after creation
        /// </summary>
        /// <param name="registeredInstance">Instance that will be created</param>
        private void registerInstance(Instance registeredInstance)
        {
            var defaultID = CreateID("$default");
            registeredInstance.SetDefaultID(defaultID);
            _createdInstances.Add(registeredInstance.ID, registeredInstance);
        }
    }
}
