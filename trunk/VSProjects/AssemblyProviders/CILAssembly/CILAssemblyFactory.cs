using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TypeSystem;

namespace AssemblyProviders.CILAssembly
{
    public class CILAssemblyFactory:AssemblyProviderFactory
    {
        public override AssemblyProvider Create(object assemblyKey)
        {
            var assemblyPath = assemblyKey as string;
            if (assemblyPath == null)
                //TODO event for path changes can be hooked - or place it into plugin ?
                return null;

            return new CILAssembly(assemblyPath);
        }
    }
}
