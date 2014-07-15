using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Composition;

using EnvDTE;

using TypeSystem;
using AssemblyProviders;
using AssemblyProviders.ProjectAssembly;
using AssemblyProviders.CILAssembly;

using Interoperability;

namespace RecommendedExtensions.AssemblyProviders
{
    [Export(typeof(ExtensionExport))]
    public class AssemblyProvidersExport : ExtensionExport
    {
        [Import(typeof(VisualStudioServices))]
        public VisualStudioServices Services;

        protected override void Register()
        {
            Message("Exporting Recommended AssemblyProviders");

            if (Services == null)
            {
                Error("Cannot export C# Assembly provider without VisualStudioServices");
            }
            else
            {
                ExportAssemblyFactory<Project>((project) => new VsProjectAssembly(project, Services));
            }

            ExportAssemblyFactory<string>((path) => new CILAssembly(path));
        }
    }
}
