using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Drawing;

namespace MEFEditor.UnitTesting.Drawing_TestUtils
{
    /// <summary>
    /// Class TestDrawingFactory.
    /// </summary>
    class TestDrawingFactory:AbstractDiagramFactory
    {
        /// <summary>
        /// Creates the content that will be own by given diagram item.
        /// </summary>
        /// <param name="owningItem">The owning item.</param>
        /// <returns>Created content drawing.</returns>
        public override ContentDrawing CreateContent(DiagramItem owningItem)
        {
            return new TestContent(owningItem);
        }

        /// <summary>
        /// Creates the join according to given definition.
        /// </summary>
        /// <param name="definition">The definition of join.</param>
        /// <param name="context">The context of <see cref="DiagramDefinition" /> where join will be displayed.</param>
        /// <returns>Join drawing.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override JoinDrawing CreateJoin(JoinDefinition definition, DiagramContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the connector according to given definition.
        /// </summary>
        /// <param name="definition">The connector definition.</param>
        /// <param name="owningItem">The owning item.</param>
        /// <returns>Created drawing.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override ConnectorDrawing CreateConnector(ConnectorDefinition definition, DiagramItem owningItem)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates content of the recursive item that will be shown at <see cref="DiagramDefinition" /> instead.
        /// </summary>
        /// <param name="item">The item which will be replaced by recursive content.</param>
        /// <returns>Created recursive content.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override ContentDrawing CreateRecursiveContent(DiagramItem item)
        {
            throw new NotImplementedException();
        }
    }
}
