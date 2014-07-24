using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

using TypeSystem.Core;
using TypeSystem.Transactions;

namespace TypeSystem
{
    /// <summary>
    /// Event detected on given component
    /// </summary>
    /// <param name="component">Component that is affected by event</param>
    public delegate void ComponentEvent(ComponentInfo component);

    /// <summary>
    /// Reports every resolved method that has been invalidated
    /// </summary>
    /// <param name="invalidatedMethod">Method that is invalidated</param>
    public delegate void MethodInvalidationEvent(MethodID invalidatedMethod);

    public class AppDomainServices
    {
        /// <summary>
        /// Manager which services are provided
        /// </summary>
        private readonly AssembliesManager _manager;

        /// <summary>
        /// All available components
        /// </summary>
        public IEnumerable<ComponentInfo> Components { get { return _manager.Components; } }

        /// <summary>
        /// All loaded assemblies
        /// </summary>
        public IEnumerable<AssemblyProvider> Assemblies { get { return _manager.Assemblies; } }

        public AssemblyLoader Loader { get { return _manager.Loader; } }

        /// <summary>
        /// Name of currently running transaction
        /// </summary>
        public string RunningTransaction { get; private set; }

        /// <summary>
        /// Name of transaction progress
        /// </summary>
        public string TransactionProgress { get; private set; }

        public TransactionManager Transactions { get { return _manager.Transactions; } }

        /// <summary>
        /// Event fired whenever new assembly is added into AppDomain
        /// </summary>
        public event AssemblyEvent AssemblyAdded;

        /// <summary>
        /// Event fired whenever assembly is removed from AppDomain
        /// </summary>
        public event AssemblyEvent AssemblyRemoved;

        public event ComponentEvent ComponentAdded;

        public event ComponentEvent ComponentRemoved;

        public event MethodInvalidationEvent MethodInvalidated;

        public event Action CompositionSchemeInvalidated;

        /// <summary>
        /// Event fired whenever new message is logged
        /// </summary>
        public event OnLogEvent OnLog;

        internal AppDomainServices(AssembliesManager manager)
        {
            _manager = manager;

            _manager.ComponentAdded += (c) =>
            {
                if (ComponentAdded != null) ComponentAdded(c);
            };

            _manager.ComponentRemoved += (c) =>
            {
                if (ComponentRemoved != null) ComponentRemoved(c);
            };

            _manager.AssemblyAdded += (a) =>
            {
                if (AssemblyAdded != null) AssemblyAdded(a);
            };

            _manager.AssemblyRemoved += (a) =>
            {
                if (AssemblyRemoved != null) AssemblyRemoved(a);
            };


            _manager.Cache.MethodInvalidated += (m) =>
            {
                if (MethodInvalidated != null) MethodInvalidated(m);
            };
        }

        internal void CompositionSchemeInvalidation()
        {
            if (CompositionSchemeInvalidated != null)
                CompositionSchemeInvalidated();
        }

        /// <summary>
        /// Get assembly which defines given method.
        /// </summary>
        /// <param name="method">Method which assembly is searched</param>
        /// <returns>Assembly provider where method is defined</returns>
        public AssemblyProvider GetDefiningAssemblyProvider(MethodID method)
        {
            return _manager.GetDefiningAssemblyProvider(method);
        }

        /// <summary>
        /// Get assembly which defines given method.
        /// </summary>
        /// <param name="method">Method which assembly is searched</param>
        /// <returns>Assembly where method is defined</returns>
        public TypeAssembly GetDefiningAssembly(MethodID method)
        {
            return _manager.GetDefiningAssembly(method);
        }

        /// <summary>
        /// Get assembly which defines given type.
        /// </summary>
        /// <param name="type">Type which assembly is searched</param>
        /// <returns>Assembly where type is defined</returns>
        public TypeAssembly GetDefiningAssembly(InstanceInfo type)
        {
            return _manager.GetDefiningAssembly(type);
        }



        #region Logging routines

        /// <summary>
        /// Method used for logging during extension registering
        /// </summary>
        /// <param name="category">Category that is registered</param>
        /// <param name="format">Format of logged entry</param>
        /// <param name="args">Format arguments</param>
        public virtual void Log(string category, string format, params object[] args)
        {
            var message = string.Format(format, args);

            if (OnLog != null)
                OnLog(category, message);
        }

        /// <summary>
        /// Method used for message logging during extension registering
        /// </summary>
        /// <param name="format">Format of logged message</param>
        /// <param name="args">Format arguments</param>
        public void Warning(string format, params object[] args)
        {
            Log("WARNING", format, args);
        }

        /// <summary>
        /// Method used for error logging during extension registering
        /// </summary>
        /// <param name="format">Format of logged error</param>
        /// <param name="args">Format arguments</param>
        public void Error(string format, params object[] args)
        {
            Log("ERROR", format, args);
        }

        #endregion
    }
}
