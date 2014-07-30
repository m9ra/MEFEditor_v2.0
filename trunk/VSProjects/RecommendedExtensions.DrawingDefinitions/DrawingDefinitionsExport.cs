using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

using MEFEditor.Drawing;
using MEFEditor.TypeSystem;
using RecommendedExtensions.Core.Drawings;

namespace RecommendedExtensions.DrawingDefinitions
{
    /// <summary>
    /// Exporting class of drawing definitions that are exposed by <see cref="RecommendedExtensions"/> to
    /// provide MEF analyzing support.
    /// </summary>
    [Export(typeof(ExtensionExport))]
    public class DrawingDefinitionsExport : ExtensionExport
    {
        /// <summary>
        /// Register drawing definitions of <see cref="RecommendedExtensions"/>.
        /// </summary>
        protected override void Register()
        {
            Message("Exporting Recommended DrawingDefinitions");

            //Component drawing
            ExportGeneralDrawing((i) => new ComponentDrawing(i));

            //Catalogs and containers drawings 
            ExportDrawing<CompositionContainer>((i) => new CompositionContainerDrawing(i));
            ExportDrawing<CompositionBatch>((i) => new CompositionBatchDrawing(i));
            ExportDrawing<DirectoryCatalog>((i) => new DirectoryCatalogDrawing(i));
            ExportDrawing<AggregateCatalog>((i) => new AggregateCatalogDrawing(i));
            ExportDrawing<TypeCatalog>((i) => new TypeCatalogDrawing(i));
            ExportDrawing<AssemblyCatalog>((i) => new AssemblyCatalogDrawing(i));

            //general drawing definition provider
            ExportGeneralDrawingDefinitionProvider(GeneralDefinitionProvider.Draw);

            //join drawing factory
            ExportJoinFactory((d) => new CompositionJoin(d));

            //connectors drawing
            ExportConnectorFactory((d, o) => new ExportConnector(d, o), ConnectorAlign.Right);
            ExportConnectorFactory((d, o) => new SelfExportConnector(d, o), ConnectorAlign.Top);
            ExportConnectorFactory((d, o) => new ImportConnector(d, o), ConnectorAlign.Left);
            //notice that this export is not required however for completeness it is added.
            ExportConnectorFactory((d, o) => new SelfExportConnector(d, o), ConnectorAlign.Bottom);
        }
    }
}
