using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Drawing
{

    /// <summary>
    /// Delegate for providing menu items
    /// </summary>
    /// <returns>IEnumerable&lt;EditDefinition&gt;.</returns>
    public delegate IEnumerable<EditDefinition> EditsMenuProvider();

    /// <summary>
    /// Delegate called on drag start event.
    /// </summary>
    /// <param name="item">The item.</param>
    public delegate void OnDragStart(DiagramItemDefinition item);

    /// <summary>
    /// Delegate called on drag end event
    /// </summary>
    public delegate void OnDragEnd();

    /// <summary>
    /// Provides API for defining diagram drawing.
    /// </summary>
    public class DiagramDefinition
    {
        /// <summary>
        /// Contained item definitions.
        /// </summary>
        private readonly Dictionary<string, DiagramItemDefinition> _definitions = new Dictionary<string, DiagramItemDefinition>();

        /// <summary>
        /// Join definitions index.
        /// </summary>
        private readonly Dictionary<string, JoinPointDefinitions> _joinPointDefintions = new Dictionary<string, JoinPointDefinitions>();

        /// <summary>
        /// Join definitions.
        /// </summary>
        private readonly HashSet<JoinDefinition> _joinDefinitions = new HashSet<JoinDefinition>();

        /// <summary>
        /// Global edits.
        /// </summary>
        private readonly List<EditDefinition> _edits = new List<EditDefinition>();

        /// <summary>
        /// Global commands.
        /// </summary>
        private readonly List<CommandDefinition> _commands = new List<CommandDefinition>();

        /// <summary>
        /// The menu providers.
        /// </summary>
        internal readonly Dictionary<string, EditsMenuProvider> MenuProviders = new Dictionary<string, EditsMenuProvider>();

        /// <summary>
        /// Gets displayed item definitions.
        /// </summary>
        /// <value>The item definitions.</value>
        public IEnumerable<DiagramItemDefinition> ItemDefinitions { get { return _definitions.Values; } }

        /// <summary>
        /// Gets displayed join definitions.
        /// </summary>
        /// <value>The join definitions.</value>
        public IEnumerable<JoinDefinition> JoinDefinitions { get { return _joinDefinitions; } }

        /// <summary>
        /// Gets the global edits.
        /// </summary>
        /// <value>The edits.</value>
        public IEnumerable<EditDefinition> Edits { get { return _edits; } }

        /// <summary>
        /// Gets the global commands.
        /// </summary>
        /// <value>The commands.</value>
        public IEnumerable<CommandDefinition> Commands { get { return _commands; } }

        /// <summary>
        /// Occurs when Item drag ends.
        /// </summary>
        public event OnDragEnd OnDragEnd;

        /// <summary>
        /// Occurs when Item drag starts.
        /// </summary>
        public event OnDragStart OnDragStart;

        /// <summary>
        /// Determine to use item avoidance.
        /// </summary>
        public bool UseItemAvoidance;

        /// <summary>
        /// Determine to use join avoidance.
        /// </summary>
        public bool UseJoinAvoidance;

        /// <summary>
        /// Determine to show join lines.
        /// </summary>
        public bool ShowJoinLines;

        /// <summary>
        /// The initial view that will be edited.
        /// </summary>
        public readonly EditViewBase InitialView;

        /// <summary>
        /// Number of defined DrawingDefinitions.
        /// </summary>
        /// <value>The count.</value>
        public int Count { get { return _definitions.Count; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramDefinition" /> class.
        /// </summary>
        /// <param name="initialView">The initial view.</param>
        public DiagramDefinition(EditViewBase initialView)
        {
            InitialView = initialView;
        }

        /// <summary>
        /// Add drawing definition into context. Given drawing
        /// definition will be displayed in output.
        /// </summary>
        /// <param name="drawing">Defined drawing.</param>
        public void DrawItem(DiagramItemDefinition drawing)
        {
            if (ContainsDrawing(drawing.ID))
                return;

            _definitions.Add(drawing.ID, drawing);
        }

        /// <summary>
        /// Determine that context already contains drawing definition for given ID.
        /// </summary>
        /// <param name="id">ID which drawing is tested.</param>
        /// <returns>True if context contains given drawing, false otherwise.</returns>
        public bool ContainsDrawing(string id)
        {
            return _definitions.ContainsKey(id);
        }

        /// <summary>
        /// Gets the item definition.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>DiagramItemDefinition.</returns>
        public DiagramItemDefinition GetItemDefinition(string id)
        {
            return _definitions[id];
        }

        /// <summary>
        /// Add join point into context. If join point with given pointKey
        /// is already defined for owningDrawing, existing join point definition
        /// is returned.
        /// </summary>
        /// <param name="owningDrawing">Reference to drawing definition containing drawed join point.</param>
        /// <param name="pointKey">Key, providing access to join point.</param>
        /// <returns>Created or existing join point for given pointKey.</returns>
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
        /// <param name="join">Join that will be drawed.</param>
        public void DrawJoin(JoinDefinition join)
        {
            _joinDefinitions.Add(join);
        }

        /// <summary>
        /// Get all defined join points for given drawing definition.
        /// </summary>
        /// <param name="definition">Drawing definition which join points will be returned.</param>
        /// <returns>Defined join points.</returns>
        internal IEnumerable<ConnectorDefinition> GetConnectorDefinitions(DiagramItemDefinition definition)
        {
            JoinPointDefinitions definitions;
            if (_joinPointDefintions.TryGetValue(definition.ID, out  definitions))
                return definitions.Values;

            return new ConnectorDefinition[0];
        }

        /// <summary>
        /// Report drag start of given item.
        /// </summary>
        /// <param name="item">The item.</param>
        internal void DragStart(DiagramItem item)
        {
            if (OnDragStart != null)
                OnDragStart(item.Definition);
        }

        /// <summary>
        /// Report drag end of given item.
        /// </summary>
        internal void DragEnd()
        {
            if (OnDragEnd != null)
                OnDragEnd();
        }

        /// <summary>
        /// Adds the global edit.
        /// </summary>
        /// <param name="edit">The edit.</param>
        public void AddEdit(EditDefinition edit)
        {
            _edits.Add(edit);
        }

        /// <summary>
        /// Add menu of edits.
        /// </summary>
        /// <param name="editName">Name of the edit.</param>
        /// <param name="provider">The provider.</param>
        public void AddEditsMenu(string editName, EditsMenuProvider provider)
        {
            MenuProviders[editName] = provider;
        }

        /// <summary>
        /// Add global command.
        /// </summary>
        /// <param name="command">The command.</param>
        public void AddCommand(CommandDefinition command)
        {
            _commands.Add(command);
        }
    }
}
