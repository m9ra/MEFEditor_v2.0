using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Composition;

using EnvDTE;

using MEFEditor.TypeSystem;
using MEFEditor.Interoperability;

using RecommendedExtensions.Core.AssemblyProviders.CILAssembly;
using RecommendedExtensions.Core.AssemblyProviders.ProjectAssembly;

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
