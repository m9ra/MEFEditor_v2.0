using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

using Utilities;

namespace Drawing
{
    public class DiagramItemDefinition
    {
        private DrawingProperties _properties = new DrawingProperties();

        private readonly List<EditDefinition> _edits = new List<EditDefinition>();

        private readonly List<CommandDefinition> _commands = new List<CommandDefinition>();

        private readonly MultiDictionary<string, EditDefinition> _attachedEdits = new MultiDictionary<string, EditDefinition>();

        private HashSet<SlotDefinition> _slots = new HashSet<SlotDefinition>();

        /// <summary>
        /// Type of drawed object. May differ from type that provides draing
        /// </summary>
        public readonly string DrawedType;

        /// <summary>
        /// ID of current drawing definition. Has to be uniqueue in drawing context scope
        /// </summary>
        public readonly string ID;

        /// <summary>
        /// Drawing reference to current diagram item
        /// </summary>
        public readonly DrawingReference Reference;

        /// <summary>
        /// All properties that has been set for drawing definition
        /// </summary>
        public IEnumerable<DrawingProperty> Properties { get { return _properties.Values; } }

        public IEnumerable<SlotDefinition> Slots { get { return _slots; } }

        public IEnumerable<EditDefinition> Edits { get { return _edits; } }

        public IEnumerable<CommandDefinition> Commands { get { return _commands; } }

        public Point GlobalPosition { get; set; }

        /// <summary>
        /// Initialize drawing definition
        /// </summary>
        /// <param name="id">ID of current drawing definition. Has to be uniqueue in drawing context scope</param>
        /// <param name="drawedType">Type of drawed object. May differ from type that provides drawing</param>
        public DiagramItemDefinition(string id, string drawedType)
        {
            ID = id;
            DrawedType = drawedType;

            Reference = new DrawingReference(ID);
        }

        /// <summary>
        /// Set property of given name to value
        /// </summary>
        /// <param name="name">Name of property</param>
        /// <param name="value">Value of property</param>
        public void SetProperty(string name, string value)
        {
            _properties[name] = new DrawingProperty(name, value); 
        }

        /// <summary>
        /// Add drawing slot to current definition
        /// </summary>
        /// <param name="slot">Added slot</param>
        public void AddSlot(SlotDefinition slot)
        {
            _slots.Add(slot);
        }

        public void AddEdit(EditDefinition editDefinition)
        {
            _edits.Add(editDefinition);
        }

        public void AddCommand(CommandDefinition command)
        {
            _commands.Add(command);
        }

        public void AttachEdit(string attachingID, EditDefinition editDefinition)
        {
            _attachedEdits.Add(attachingID, editDefinition);
        }

        internal IEnumerable<EditDefinition> GetAttachedEdits(string attachingID)
        {
            return _attachedEdits.Get(attachingID);
        }

        public DrawingProperty GetProperty(string name)
        {
            DrawingProperty result;
            _properties.TryGetValue(name, out result);

            return result;
        }

        public string GetPropertyValue(string name)
        {
            var property = GetProperty(name);
            if (property == null)
                return null;

            return property.Value;
        }
    }
}
