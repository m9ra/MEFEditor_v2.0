using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Drawing;

using MEFAnalyzers.Drawings;

namespace MEFEditor.Plugin.Drawing
{
    class DiagramFactory:AbstractDiagramFactory
    {
        
        private ContentDrawer _defaultContentDrawer;

        private Dictionary<string, ContentDrawer> _contentDrawers = new Dictionary<string, ContentDrawer>();

        internal DiagramFactory(params ContentDrawer[] drawers)
        {
            foreach (var drawer in drawers)
            {
                if (drawer.IsDefaultDrawer)
                {
                    _defaultContentDrawer = drawer;
                }
                else
                {
                    _contentDrawers.Add(drawer.DrawedType, drawer);
                }
            }
        }

        public override ContentDrawing CreateContent(DiagramItem owningItem)
        {
            var definition = owningItem.Definition;

            ContentDrawer drawer;
            if (_contentDrawers.TryGetValue(definition.DrawedType, out drawer))
                return drawer.Provider(owningItem);

            return _defaultContentDrawer.Provider(owningItem);
        }

        public override JoinDrawing CreateJoin(JoinDefinition definition, DiagramContext context)
        {
            return new CompositionJoin(definition);
        }

        public override ConnectorDrawing CreateConnector(ConnectorDefinition definition, DiagramItem owningItem)
        {
            var kind=definition.GetProperty("Kind");
            switch (kind.Value)
            {
                case "Import":
                    return new ImportConnector(definition, owningItem);
                case "SelfExport":
                    return new SelfExportConnector(definition, owningItem);
                case "Export":
                    return new ExportConnector(definition, owningItem);
                default:
                    throw new NotSupportedException(kind.Value);
            }
        }
    }
}
