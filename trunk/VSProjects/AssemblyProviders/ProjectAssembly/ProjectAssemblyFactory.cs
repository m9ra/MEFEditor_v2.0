using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EnvDTE;
using TypeSystem;
using Interoperability;

namespace AssemblyProviders.ProjectAssembly
{
    public class ProjectAssemblyFactory : AssemblyProviderFactory
    {
        private readonly VisualStudioServices _vs;

        public ProjectAssemblyFactory(VisualStudioServices vs)
        {
            _vs = vs;
        }

        public override AssemblyProvider Create(object assemblyKey)
        {
            var project = assemblyKey as Project;

            if (project == null)
                return null;

            return new VsProjectAssembly(project, _vs);
        }
    }
}
