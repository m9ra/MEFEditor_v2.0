using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

using Utilities;

namespace MEFEditor.Drawing
{
    /// <summary>
    /// Definition of <see cref="DiagramItem" /> that can be displayed on diagram.
    /// </summary>
    public class DiagramItemDefinition
    {
        /// <summary>
        /// The properties of item.
        /// </summary>
        private DrawingProperties _properties = new DrawingProperties();

        /// <summary>
        /// The edits of item.
        /// </summary>
        private readonly List<EditDefinition> _edits = new List<EditDefinition>();

        /// <summary>
        /// The commands of item.
        /// </summary>
        private readonly List<CommandDefinition> _commands = new List<CommandDefinition>();

        /// <summary>
        /// The attached edits of item.
        /// </summary>
        private readonly MultiDictionary<string, EditDefinition> _attachedEdits = new MultiDictionary<string, EditDefinition>();

        /// <summary>
        /// The slots of item.
        /// </summary>
        private HashSet<SlotDefinition> _slots = new HashSet<SlotDefinition>();

        /// <summary>
        /// Type of drawn object. May differ from type that provides drawing.
        /// </summary>
        public readonly string DrawedType;

        /// <summary>
        /// ID of current drawing definition. Has to be unique in drawing context scope.
        /// </summary>
        public readonly string ID;

        /// <summary>
        /// Drawing reference to current diagram item.
        /// </summary>
        public readonly DrawingReference Reference;

        /// <summary>
        /// All properties that has been set for drawing definition.
        /// </summary>
        /// <value>The properties.</value>
        public IEnumerable<DrawingProperty> Properties { get { return _properties.Values; } }

        /// <summary>
        /// Gets the slots of item.
        /// </summary>
        /// <value>The slots.</value>
        public IEnumerable<SlotDefinition> Slots { get { return _slots; } }

        /// <summary>
        /// Gets the edits of item.
        /// </summary>
        /// <value>The edits.</value>
        public IEnumerable<EditDefinition> Edits { get { return _edits; } }

        /// <summary>
        /// Gets the commands of item.
        /// </summary>
        /// <value>The commands.</value>
        public IEnumerable<CommandDefinition> Commands { get { return _commands; } }

        /// <summary>
        /// The initial global position of item that can be defined.
        /// </summary>
        public Point? GlobalPosition;

        /// <summary>
        /// Initialize drawing definition.
        /// </summary>
        /// <param name="id">ID of current drawing definition. Has to be uniqueue in drawing context scope.</param>
        /// <param name="drawedType">Type of drawed object. May differ from type that provides drawing.</param>
        public DiagramItemDefinition(string id, string drawedType)
        {
            ID = id;
            DrawedType = drawedType;

            Reference = new DrawingReference(ID);
        }

        /// <summary>
        /// Set property of given name to value.
        /// </summary>
        /// <param name="name">Name of property.</param>
        /// <param name="value">Value of property.</param>
        public void SetProperty(string name, string value)
        {
            _properties[name] = new DrawingProperty(name, value); 
        }

        /// <summary>
        /// Add drawing slot to current definition.
        /// </summary>
        /// <param name="slot">Added slot.</param>
        public void AddSlot(SlotDefinition slot)
        {
            _slots.Add(slot);
        }

        /// <summary>
        /// Adds the edit of item.
        /// </summary>
        /// <param name="editDefinition">The edit definition.</param>
        public void AddEdit(EditDefinition editDefinition)
        {
            _edits.Add(editDefinition);
        }

        /// <summary>
        /// Adds the command of item.
        /// </summary>
        /// <param name="command">The command.</param>
        public void AddCommand(CommandDefinition command)
        {
            _commands.Add(command);
        }

        /// <summary>
        /// Attach edit for item with attachingID.
        /// </summary>
        /// <param name="attachingID">The attaching identifier.</param>
        /// <param name="editDefinition">The edit definition.</param>
        public void AttachEdit(string attachingID, EditDefinition editDefinition)
        {
            _attachedEdits.Add(attachingID, editDefinition);
        }

        /// <summary>
        /// Gets the attached edits.
        /// </summary>
        /// <param name="attachingID">The attaching identifier.</param>
        /// <returns>IEnumerable&lt;EditDefinition&gt;.</returns>
        internal IEnumerable<EditDefinition> GetAttachedEdits(string attachingID)
        {
            return _attachedEdits.Get(attachingID);
        }

        /// <summary>
        /// Gets the property accoring to its name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>DrawingProperty.</returns>
        public DrawingProperty GetProperty(string name)
        {
            DrawingProperty result;
            _properties.TryGetValue(name, out result);

            return result;
        }

        /// <summary>
        /// Gets the property value according to its name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Property value.</returns>
        public string GetPropertyValue(string name)
        {
            var property = GetProperty(name);
            if (property == null)
                return null;

            return property.Value;
        }
    }
}
