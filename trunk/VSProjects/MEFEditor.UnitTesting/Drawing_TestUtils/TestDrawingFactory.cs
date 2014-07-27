using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Drawing;

namespace MEFEditor.UnitTesting.Drawing_TestUtils
{
    class TestDrawingFactory:AbstractDiagramFactory
    {
        public override ContentDrawing CreateContent(DiagramItem owningItem)
        {
            return new TestContent(owningItem);
        }

        public override JoinDrawing CreateJoin(JoinDefinition definition, DiagramContext context)
        {
            throw new NotImplementedException();
        }

        public override ConnectorDrawing CreateConnector(ConnectorDefinition definition, DiagramItem owningItem)
        {
            throw new NotImplementedException();
        }
        
        public override ContentDrawing CreateRecursiveContent(DiagramItem item)
        {
            throw new NotImplementedException();
        }
    }
}
