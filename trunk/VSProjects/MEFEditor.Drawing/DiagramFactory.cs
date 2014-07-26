using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

namespace MEFEditor.Drawing
{
    public abstract class AbstractDiagramFactory
    {
        public abstract ContentDrawing CreateContent(DiagramItem owningItem);

        public abstract JoinDrawing CreateJoin(JoinDefinition definition, DiagramContext context);

        public abstract ConnectorDrawing CreateConnector(ConnectorDefinition definition, DiagramItem owningItem);

        public abstract ContentDrawing CreateRecursiveContent(DiagramItem item);
    }
}
