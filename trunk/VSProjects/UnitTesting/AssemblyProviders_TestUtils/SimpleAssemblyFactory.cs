using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TypeSystem;

using AssemblyProviders.CILAssembly;

namespace UnitTesting.AssemblyProviders_TestUtils
{
    public class SimpleAssemblyFactory : AssemblyProviderFactory
    {
        /// <summary>
        /// Providers that will be "created" by this factory
        /// </summary>
        private Dictionary<object, AssemblyProvider> _providers = new Dictionary<object, AssemblyProvider>();

        public void Register(object key, AssemblyProvider provider)
        {
            _providers.Add(key, provider);
        }

        public override AssemblyProvider Create(object assemblyKey)
        {
            AssemblyProvider explicitProvider;
            if (_providers.TryGetValue(assemblyKey, out explicitProvider))
                return explicitProvider;


            var path = assemblyKey as string;
            if (path != null)
                return new CILAssembly(path);

            return null;
        }
    }
}
