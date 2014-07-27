using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MEFEditor.TypeSystem
{
    /// <summary>
    /// Factory used by <see cref="AssemblyLoader"/> for creating <see cref="AssemblyProvider"/> objects.
    /// </summary>
    public abstract class AssemblyProviderFactory
    {
        /// <summary>
        /// Create <see cref="AssemblyProvider"/> from given key
        /// </summary>
        /// <param name="assemblyKey">Key defining reference</param>
        /// <returns></returns>
        public abstract AssemblyProvider Create(object assemblyKey);
    }
}
