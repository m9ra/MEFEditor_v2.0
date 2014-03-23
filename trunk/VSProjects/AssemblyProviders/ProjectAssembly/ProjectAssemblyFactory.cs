using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EnvDTE;
using TypeSystem;

namespace AssemblyProviders.ProjectAssembly
{
    public class ProjectAssemblyFactory : AssemblyProviderFactory
    {

        public override AssemblyProvider Create(object assemblyKey)
        {
            var project = assemblyKey as Project;

            if (project == null)
                return null;

            return new VsProjectAssembly(project);
        }
    }
}
