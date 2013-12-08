using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drawing
{

    public delegate void OnDragStart(DiagramItemDefinition item);

    public delegate void OnDragEnd();

    /// <summary>
    /// Provides API for defining drawing
    /// </summary>
    public class DiagramDefinition
    {
        private readonly Dictionary<string, DiagramItemDefinition> _definitions = new Dictionary<string, DiagramItemDefinition>();

        private readonly Dictionary<string, JoinPointDefinitions> _joinPointDefintions = new Dictionary<string, JoinPointDefinitions>();

        private readonly HashSet<JoinDefinition> _joinDefinitions = new HashSet<JoinDefinition>();

        public IEnumerable<DiagramItemDefinition> ItemDefinitions { get { return _definitions.Values; } }

        public IEnumerable<JoinDefinition> JoinDefinitions { get { return _joinDefinitions; } }

        public event OnDragEnd OnDragEnd;

        public event OnDragStart OnDragStart;

        public readonly EditViewBase InitialView;

        /// <summary>
        /// Number of defined DrawingDefinitions
        /// </summary>
        public int Count { get { return _definitions.Count; } }

        public DiagramDefinition(EditViewBase initialView)
        {
            InitialView = initialView;
        }

        /// <summary>
        /// Add drawing definition into context. Given drawing
        /// definition will be displayed in output
        /// </summary>
        /// <param name="drawing">Defined drawing</param>
        public void DrawItem(DiagramItemDefinition drawing)
        {
            if (ContainsDrawing(drawing.ID))
                return;

            _definitions.Add(drawing.ID, drawing);
        }

        /// <summary>
        /// Determine that context already contains drawing definition for given ID
        /// </summary>
        /// <param name="id">ID which drawing is tested</param>
        /// <returns>True if context contains given drawing, false otherwise</returns>
        public bool ContainsDrawing(string id)
        {
            return _definitions.ContainsKey(id);
        }

        public DiagramItemDefinition GetItemDefinition(string id)
        {
            return _definitions[id];
        }

        /// <summary>
        /// Add join point into context. If join point with given pointKey
        /// is already defined for owningDrawing, existing join point definition
        /// is returned.
        /// </summary>
        /// <param name="owningDrawing">Reference to drawing definition containing drawed join point</param>
        /// <param name="pointKey">Key, providing access to join point</param>
        /// <returns>Created or existing join point for given pointKey</returns>
        public ConnectorDefinition DrawJoinPoint(DrawingReference owningDrawing, object pointKey)
        {
            JoinPointDefinitions joins;

            if (!_joinPointDefintions.TryGetValue(owningDrawing.DefinitionID, out joins))
            {
                joins = new JoinPointDefinitions();
                _joinPointDefintions[owningDrawing.DefinitionID] = joins;
            }

            ConnectorDefinition result;
            if (!joins.TryGetValue(pointKey, out result))
            {
                result = new ConnectorDefinition(owningDrawing);
                joins[pointKey] = result;
            }

            return result;
        }

        /// <summary>
        /// Add join into context.
        /// </summary>
        /// <param name="join">Join that will be drawed</param>
        public void DrawJoin(JoinDefinition join)
        {
            _joinDefinitions.Add(join);
        }

        /// <summary>
        /// Get all defined join points for given drawing definition
        /// </summary>
        /// <param name="definition">Drawing definition which join points will be returned</param>
        /// <returns>Defined join points</returns>
        internal IEnumerable<ConnectorDefinition> GetConnectorDefinitions(DiagramItemDefinition definition)
        {
            JoinPointDefinitions definitions;
            if (_joinPointDefintions.TryGetValue(definition.ID, out  definitions))
                return definitions.Values;

            return new ConnectorDefinition[0];
        }

        internal void DragStart(DiagramItem item)
        {
            if (OnDragStart != null)
                OnDragStart(item.Definition);
        }

        internal void DragEnd()
        {
            if (OnDragEnd != null)
                OnDragEnd();
        }


    }
}
