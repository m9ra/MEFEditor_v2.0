using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

using Drawing;
using TypeSystem;
using MEFAnalyzers.Drawings;

namespace RecommendedExtensions.DrawingDefinitions
{
    [Export(typeof(ExtensionExport))]
    public class DrawingDefinitionsExport : ExtensionExport
    {
        protected override void Register()
        {
            Message("Exporting Recommended DrawingDefinitions");

            //Component drawing
            ExportGeneralDrawing((i) => new ComponentDrawing(i));

            //Catalogs and containers drawings
            ExportDrawing<CompositionContainer>((i) => new CompositionContainerDrawing(i));
            ExportDrawing<CompositionBatchDrawing>((i) => new CompositionBatchDrawing(i));
            ExportDrawing<DirectoryCatalog>((i) => new DirectoryCatalogDrawing(i));
            ExportDrawing<AggregateCatalog>((i)=>new AggregateCatalogDrawing(i));
            ExportDrawing<TypeCatalog>((i)=>new TypeCatalogDrawing(i));
            ExportDrawing<AssemblyCatalog>((i)=>new AssemblyCatalogDrawing(i));
        }
    }
}
