using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using MEFEditor.TypeSystem;

using RecommendedExtensions.Core.AssemblyProviders.CILAssembly;

namespace MEFEditor.UnitTesting.AssemblyProviders_TestUtils
{
    /// <summary>
    /// Simple assembly factory that can create assemblies
    /// by using <see cref="AssemblyProvider"/> implementations that
    /// are registered with specified keys. It is used for testing purposes.
    /// </summary>
    public class SimpleAssemblyFactory : AssemblyProviderFactory
    {
        /// <summary>
        /// Providers that will be "created" by this factory.
        /// </summary>
        private Dictionary<object, AssemblyProvider> _providers = new Dictionary<object, AssemblyProvider>();

        /// <summary>
        /// Registers the specified <see cref="AssemblyProvider"/> with given key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="provider">The provider.</param>
        public void Register(object key, AssemblyProvider provider)
        {
            _providers.Add(key, provider);
        }

        /// <summary>
        /// Create <see cref="AssemblyProvider" /> from given key
        /// </summary>
        /// <param name="assemblyKey">Key defining reference</param>
        /// <returns>AssemblyProvider.</returns>
        public override AssemblyProvider Create(object assemblyKey)
        {
            AssemblyProvider explicitProvider;
            if (_providers.TryGetValue(assemblyKey, out explicitProvider))
                return explicitProvider;


            var path = assemblyKey as string;
            if (path != null && File.Exists(path))
                return new CILAssembly(path);

            return null;
        }
    }
}
