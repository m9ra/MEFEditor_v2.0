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
        private readonly List<Instance> _createdInstances = new List<Instance>();

        /// <summary>
        /// All id's used during last execution
        /// </summary>
        private readonly HashSet<string> _usedIDs = new HashSet<string>();

        /// <summary>
        /// Settings available for virtual machine
        /// </summary>
        internal readonly IMachineSettings Settings;

        /// <summary>
        /// Initialize machine with specified settings
        /// </summary>
        /// <param name="settings">Settings specified for machine</param>
        public Machine(IMachineSettings settings)
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
        /// Run analysis of program loaded via given loader. Execution starts from loader.EntryPoint
        /// </summary>
        /// <param name="loader">Loader which provides instrution generation and type/methods resolving</param>
        /// <returns>Result of analysis</returns>
        public AnalyzingResult Run(LoaderBase loader, params Instance[] arguments)
        {
            return run(loader, arguments);
        }

        internal string CreateID(string hint)
        {
            var currentID = hint;
            var number = 0;
            while (_usedIDs.Contains(currentID))
            {
                currentID = string.Format("{0}_{1}", hint, number);
                ++number;
            }
            
            _usedIDs.Add(currentID);

            return currentID;
        }

        /// <summary>
        /// Run instructions present in _cachedLoader
        /// </summary>
        /// <returns>Result of analysis</returns>
        private AnalyzingResult run(LoaderBase loader, params Instance[] arguments)
        {
            _usedIDs.Clear();
            _createdInstances.Clear();
            var context = new Execution.AnalyzingContext(this, loader);
            context.DynamicCall("EntryPoint", loader.EntryPoint, arguments);

            //instance processing
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

            return context.GetResult(_createdInstances.ToArray());
        }

        /// <summary>
        /// All instances are registered after creation
        /// </summary>
        /// <param name="registeredInstance">Instance that will be created</param>
        private void registerInstance(Instance registeredInstance)
        {
            var defaultID = CreateID("$default");
            registeredInstance.SetDefaultID(defaultID);
            _createdInstances.Add(registeredInstance);
            Settings.InstanceCreated(registeredInstance);
        }
    }
}
