using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.ComponentModel.Composition;

using EnvDTE;

using MEFEditor.TypeSystem;
using MEFEditor.Interoperability;

using RecommendedExtensions.Core.Languages.CSharp;
using RecommendedExtensions.Core.AssemblyProviders.CILAssembly;
using RecommendedExtensions.Core.AssemblyProviders.CSharpAssembly;

namespace RecommendedExtensions.AssemblyProviders
{
    /// <summary>
    /// Exporting class of assembly providers that are exposed by <see cref="RecommendedExtensions"/> to
    /// provide MEF analyzing support.
    /// </summary>
    [Export(typeof(ExtensionExport))]
    public class AssemblyProvidersExport : ExtensionExport
    {
        /// <summary>
        /// Import <see cref="VisualStudioServices"/> that will be used by assembly providers.
        /// </summary>
        [Import(typeof(VisualStudioServices))]
        public VisualStudioServices Services;

        /// <summary>
        /// Register C# and CIL assembly providers of <see cref="RecommendedExtensions"/>.
        /// </summary>
        protected override void Register()
        {
            Message("Exporting Recommended AssemblyProviders");

            if (Services == null)
            {
                Error("Cannot export C# Assembly provider without VisualStudioServices");
            }
            else
            {
                ExportAssemblyFactory<Project>((project) =>
                {
                    var language = project.CodeModel.Language;
                    if (language == CSharpSyntax.LanguageID)
                        return new CSharpAssembly(project, Services);
                    else
                        return null;
                });
            }

            ExportAssemblyFactory<string>((path) =>
            {
                if (File.Exists(path) && path.EndsWith(".dll") || path.EndsWith(".exe"))
                    return new CILAssembly(path);
                else
                    return null;
            });
        }
    }
}
