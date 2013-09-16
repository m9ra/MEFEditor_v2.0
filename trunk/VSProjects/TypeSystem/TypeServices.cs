using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeSystem.Core;

using Analyzing;

namespace TypeSystem
{
    /// <summary>
    /// Provides access to information about loaded assemblies
    /// </summary>
    public class TypeServices
    {
        private readonly AssembliesManager _manager;

        internal TypeServices(AssembliesManager manager)
        {
            _manager = manager;
        }

        public MethodSearcher CreateSearcher()
        {
            return _manager.CreateSearcher();
        }

        public ComponentInfo GetComponentInfo(Instance instance)
        {
            return _manager.GetComponentInfo(instance);
        }
    }
}
