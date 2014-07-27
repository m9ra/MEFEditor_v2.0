using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

namespace MEFEditor.Drawing
{
    /// <summary>
    /// Abstract factory class for diagram primitives.
    /// </summary>
    public abstract class AbstractDiagramFactory
    {
        /// <summary>
        /// Creates the content that will be own by given diagram item.
        /// </summary>
        /// <param name="owningItem">The owning item.</param>
        /// <returns>Created content drawing.</returns>
        public abstract ContentDrawing CreateContent(DiagramItem owningItem);

        /// <summary>
        /// Creates the join according to given definition.
        /// </summary>
        /// <param name="definition">The definition of join.</param>
        /// <param name="context">The context of <see cref="DiagramDefinition"/> where join will be displayed.</param>
        /// <returns>Join drawing.</returns>
        public abstract JoinDrawing CreateJoin(JoinDefinition definition, DiagramContext context);

        /// <summary>
        /// Creates the connector according to given definition.
        /// </summary>
        /// <param name="definition">The connector definition.</param>
        /// <param name="owningItem">The owning item.</param>
        /// <returns>Created drawing.</returns>
        public abstract ConnectorDrawing CreateConnector(ConnectorDefinition definition, DiagramItem owningItem);

        /// <summary>
        /// Creates content of the recursive item that will be shown at <see cref="DiagramDefinition"/> instead.
        /// </summary>
        /// <param name="item">The item which will be replaced by recursive content.</param>
        /// <returns>Created recursive content.</returns>
        public abstract ContentDrawing CreateRecursiveContent(DiagramItem item);
    }
}
