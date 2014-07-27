using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MEFEditor.TypeSystem;   

namespace MEFEditor.Plugin.Main
{
    /// <summary>
    /// Factory used for providing exported <see cref="AssemblyProvider"/> factories
    /// </summary>
    class ExportedProvidersFactory:AssemblyProviderFactory
    {
        /// <summary>
        /// Available exported factories
        /// </summary>
        private readonly ExportedAssemblyProviderFactory[] _factories;

        internal ExportedProvidersFactory(IEnumerable<ExportedAssemblyProviderFactory> factories)
        {
            _factories = factories.ToArray();
        }

        /// <inheritdoc />
        public override AssemblyProvider Create(object assemblyKey)
        {
            for (var i = 0; i < _factories.Length; ++i)
            {
                var provider = _factories[i](assemblyKey);
                if (provider != null)
                    return provider;
            }

            return null;
        }
    }
}
