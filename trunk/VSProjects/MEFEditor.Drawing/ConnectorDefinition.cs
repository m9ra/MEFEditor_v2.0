using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Drawing
{
    /// <summary>
    /// Definition of connector that can be drawn at <see cref="MEFEditor.Drawing.DiagramCanvas" />.
    /// </summary>
    public class ConnectorDefinition
    {
        /// <summary>
        /// The properties of connector.
        /// </summary>
        private readonly DrawingProperties _properties = new DrawingProperties();

        /// <summary>
        /// The reference to the connector.
        /// </summary>
        public readonly DrawingReference Reference;

        /// <summary>
        /// Gets the properties of current connector.
        /// </summary>
        /// <value>The properties.</value>
        public IEnumerable<DrawingProperty> Properties { get { return _properties.Values; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorDefinition" /> class.
        /// </summary>
        /// <param name="reference">The reference to the connector.</param>
        public ConnectorDefinition(DrawingReference reference)
        {
            Reference = reference;
        }

        /// <summary>
        /// Sets the connector's property.
        /// </summary>
        /// <param name="property">The property.</param>
        public void SetProperty(DrawingProperty property)
        {
            _properties[property.Name] = property;
        }

        /// <summary>
        /// Sets the connector's property.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="value">The property value.</param>
        public void SetProperty(string name, string value)
        {
            SetProperty(new DrawingProperty(name, value));
        }

        /// <summary>
        /// Gets the property according to its name.
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
        /// <param name="name">The property name.</param>
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
