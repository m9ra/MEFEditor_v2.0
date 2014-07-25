//#define PassExceptions //TODO should be defined only for debuging purposes

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution;

namespace Analyzing
{

    /// <summary>
    /// Initializer used for context initialization
    /// </summary>
    /// <param name="context">Context that will be initialized</param>
    internal delegate void ContextInitializer(AnalyzingContext context);

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
        public readonly MachineSettingsBase Settings;

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
        /// Run analysis of program loaded via given loader. Execution starts from loader.EntryPoint
        /// </summary>
        /// <param name="loader">Loader which provides instrution generation and type/methods resolving</param>
        /// <returns>Result of analysis</returns>
        public AnalyzingResult Run(LoaderBase loader, MethodID entryMethod, params Instance[] arguments)
        {
            return contextInvoker(loader, (context) =>
            {
                foreach (var argument in arguments)
                {
                    if (argument != null)
                    {
                        argument.IsEntryInstance = true;
                        _createdInstances.Add(argument.ID, argument);
                    }
                }
                context.FetchCall(entryMethod, arguments);
            });
        }


        /// <summary>
        /// Run analysis of program loaded via given loader. Execution starts from loader.EntryPoint
        /// </summary>
        /// <param name="loader">Loader which provides instrution generation and type/methods resolving</param>
        /// <returns>Result of analysis</returns>
        public AnalyzingResult Run(LoaderBase loader, GeneratorBase entryMethodGenerator)
        {
            return contextInvoker(loader, (context) =>
            {
                context.PushCall(new MethodID(".$@.EntryMethod", false), entryMethodGenerator, new Instance[0]);
            });
        }

        /// <summary>
        /// Creates unique ID for <see cref="Instance"/> according to given hint
        /// </summary>
        /// <param name="hint">Hint for created id</param>
        /// <returns>Created ID</returns>
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

        /// <summary>
        /// Report change of <see cref="Instance"/> ID and refresh 
        /// index according to new ID.
        /// </summary>
        /// <param name="oldID">Old ID of instance that has been changed</param>
        internal void ReportIDChange(string oldID)
        {
            var instance = _createdInstances[oldID];
            _createdInstances.Add(instance.ID, instance);
            _createdInstances.Remove(oldID);
        }

        /// <summary>
        /// Invoke context that is initialized by given initializer
        /// </summary>
        /// <param name="loader">Loader used for invoking</param>
        /// <param name="initializer">Initializer of context used for invoking</param>
        /// <returns>Result of analyzing</returns>
        private AnalyzingResult contextInvoker(LoaderBase loader, ContextInitializer initializer)
        {
            Settings.FireBeforeInterpretation();

            _createdInstances.Clear();
            var context = new AnalyzingContext(this, loader);

            Exception runtimeException = null;

#if !PassExceptions
            try
            {
#endif
                initializer(context);
                runContext(context);
#if !PassExceptions
            }
            catch (Exception ex)
            {
                if (!Settings.CatchExceptions)
                    //we have to pass the exception to the caller
                    throw;

                runtimeException = ex;
            }
#endif

            var result = context.GetResult(new Dictionary<string, Instance>(_createdInstances));
            result.RuntimeException = runtimeException;

            Settings.FireAfterInterpretation();
            _createdInstances.Clear();

            return result;
        }

        /// <summary>
        /// Run instruction described by given context
        /// </summary>
        /// <param name="context">Properly initialized context</param>        
        private void runContext(AnalyzingContext context)
        {
            var executionLimit = Settings.ExecutionLimit;

            //interpretation processing
            while (!context.IsExecutionEnd)
            {
                //limit execution
                --executionLimit;
                if (executionLimit < 0)
                    throw new NotSupportedException("Execution limit has been reached");

                //process instruction
                var instruction = context.NextInstruction();
                if (instruction == null)
                {
                    break;
                }

                context.Prepare(instruction);
                instruction.Execute(context);
            }
        }

        /// <summary>
        /// All instances are registered after creation
        /// </summary>
        /// <param name="registeredInstance">Instance that will be created</param>
        private void registerInstance(Instance registeredInstance)
        {
            if (_createdInstances.Count > Settings.InstanceLimit)
                throw new NotSupportedException("Maximum number of created instances has been reached");

            var defaultID = CreateID('$' + registeredInstance.Info.DefaultIdHint);
            registeredInstance.SetDefaultID(defaultID);
            _createdInstances.Add(registeredInstance.ID, registeredInstance);
        }
    }
}
