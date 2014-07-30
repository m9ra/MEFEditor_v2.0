using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Drawing;
using MEFEditor.TypeSystem;

namespace MEFEditor.Plugin.Drawing
{
    /// <summary>
    /// Class DiagramFactory.
    /// </summary>
    class DiagramFactory : AbstractDiagramFactory
    {
        /// <summary>
        /// The default content drawer.
        /// </summary>
        private readonly ContentDrawer _defaultContentDrawer;

        /// <summary>
        /// Factory for creating joins.
        /// </summary>
        private readonly JoinFactory _joinFactory;

        /// <summary>
        /// Factories for creating connectors.
        /// </summary>
        private readonly ConnectorFactory[] _connectorFactories;

        /// <summary>
        /// The content drawers.
        /// </summary>
        private readonly Dictionary<string, ContentDrawer> _contentDrawers = new Dictionary<string, ContentDrawer>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramFactory" /> class.
        /// </summary>
        /// <param name="drawers">The drawers.</param>
        /// <param name="connectorFactories">Factory for creating connectors.</param>
        /// <param name="joinFactory"> Factory for creating joins.</param>
        internal DiagramFactory(JoinFactory joinFactory, ConnectorFactory[] connectorFactories, params ContentDrawer[] drawers)
        {
            _joinFactory = joinFactory;
            _connectorFactories = connectorFactories.ToArray();

            if (drawers == null)
                return;

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

        /// <summary>
        /// Creates the content that will be own by given diagram item.
        /// </summary>
        /// <param name="owningItem">The owning item.</param>
        /// <returns>Created content drawing.</returns>
        public override ContentDrawing CreateContent(DiagramItem owningItem)
        {
            var definition = owningItem.Definition;

            ContentDrawer drawer;
            if (_contentDrawers.TryGetValue(definition.DrawedType, out drawer))
                return drawer.Provider(owningItem);

            if (_defaultContentDrawer == null)
                return null;

            return _defaultContentDrawer.Provider(owningItem);
        }

        /// <summary>
        /// Creates the join according to given definition.
        /// </summary>
        /// <param name="definition">The definition of join.</param>
        /// <param name="context">The context of <see cref="DiagramDefinition" /> where join will be displayed.</param>
        /// <returns>Join drawing.</returns>
        public override JoinDrawing CreateJoin(JoinDefinition definition, DiagramContext context)
        {
            if (_joinFactory == null)
                return null;

            return _joinFactory(definition);
        }

        /// <summary>
        /// Creates the connector according to given definition.
        /// </summary>
        /// <param name="definition">The connector definition.</param>
        /// <param name="owningItem">The owning item.</param>
        /// <returns>Created drawing.</returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public override ConnectorDrawing CreateConnector(ConnectorDefinition definition, DiagramItem owningItem)
        {
            var kind = definition.GetProperty("Kind");
            switch (kind.Value)
            {
                case "Import":
                    return _connectorFactories[(int)ConnectorAlign.Left](definition, owningItem);
                case "SelfExport":
                    return _connectorFactories[(int)ConnectorAlign.Top](definition, owningItem);
                case "Export":
                    return _connectorFactories[(int)ConnectorAlign.Right](definition, owningItem);
                default:
                    throw new NotSupportedException(kind.Value);
            }
        }

        /// <summary>
        /// Creates content of the recursive item that will be shown at <see cref="DiagramDefinition" /> instead.
        /// </summary>
        /// <param name="item">The item which will be replaced by recursive content.</param>
        /// <returns>Created recursive content.</returns>
        public override ContentDrawing CreateRecursiveContent(DiagramItem item)
        {
            return new RecursiveDrawing(item);
        }
    }
}
