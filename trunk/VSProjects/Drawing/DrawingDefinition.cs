using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drawing
{
    public class DrawingDefinition
    {
        private Dictionary<string, DrawingProperty> _properties = new Dictionary<string, DrawingProperty>();

        private HashSet<DrawingSlot> _slots = new HashSet<DrawingSlot>();

        /// <summary>
        /// Type of drawed object. May differ from type that provides draing
        /// </summary>
        public readonly string DrawedType;

        /// <summary>
        /// ID of current drawing definition. Has to be uniqueue in drawing context scope
        /// </summary>
        public readonly string ID;

        /// <summary>
        /// All properties that has been set for drawing definition
        /// </summary>
        public IEnumerable<DrawingProperty> Properties { get { return _properties.Values; } }

        /// <summary>
        /// Initialize drawing definition
        /// </summary>
        /// <param name="id">ID of current drawing definition. Has to be uniqueue in drawing context scope</param>
        /// <param name="drawedType">Type of drawed object. May differ from type that provides drawing</param>
        public DrawingDefinition(string id, string drawedType)
        {
            ID = id;
            DrawedType = drawedType;
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
        public void AddSlot(DrawingSlot slot)
        {
            _slots.Add(slot);
        }
    }
}
