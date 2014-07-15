using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.ComponentModel.Composition;

using Drawing;
using TypeSystem;

namespace UserExtensions
{
    [Export(typeof(ExtensionExport))]
    public class UserExtensionsExport : ExtensionExport
    {
        protected override void Register()
        {
            //zpráva, která se zobrazí v logu editoru
            Message("Exporting UserExtensions");

            //exportujeme poskytovatele assembly
            ExportAssemblyFactory<string>(assemblyFactory);

            //exportujeme pozměněnou definici pro string
            ExportDefinition(new StringDefinition());

            //exportujeme definici pro Diagnostic i s definici vykreslení
            ExportDefinitionWithDrawing<DiagnosticDefinition>((item) => new DiagnosticDrawing(item));
        }

        private AssemblyProvider assemblyFactory(string path)
        {
            //zkontrolujeme, zda cesta má příponu .test
            var isTestAssembly = Path.GetExtension(path) == ".test";
            if (!isTestAssembly)
                //cesta má jiný tvar, než požadujeme
                return null;

            //vytvoříme poskytovatele assembly
            return new SimpleAssemblyProvider(path);
        }
    }
}
