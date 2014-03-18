using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeSystem.Core;

namespace TypeSystem
{
    public delegate void ComponentEvent(ComponentInfo component);

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

        /// <summary>
        /// Name of currently running transaction
        /// </summary>
        public string RunningTransaction { get; private set; }

        /// <summary>
        /// Name of transaction progress
        /// </summary>
        public string TransactionProgress { get; private set; }

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

        public event Action TransactionStarted;

        public event Action TransactionEnded;

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
        }
    }
}
