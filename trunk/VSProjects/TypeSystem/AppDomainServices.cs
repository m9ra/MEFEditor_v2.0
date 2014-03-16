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
        private readonly AssembliesManager _manager;

        public AssemblyCollectionBase Assemblies { get { return _manager.Assemblies; } }

        public IEnumerable<ComponentInfo> Components { get { return _manager.Components; } }

        public string RunningTransaction { get; private set; }

        public string TransactionProgress { get; private set; }

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
        }
    }
}
