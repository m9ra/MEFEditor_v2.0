using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Drawing
{
    /// <summary>
    /// Definition of <see cref="JoinDrawing" /> that will be displayed in the canvas.
    /// </summary>
    public class JoinDefinition
    {
        /// <summary>
        /// Defined drawing properties.
        /// </summary>
        private readonly DrawingProperties _joinProperties = new DrawingProperties();

        /// <summary>
        /// Source connector definition.
        /// </summary>
        public readonly ConnectorDefinition From;

        /// <summary>
        /// Target connector definition.
        /// </summary>
        public readonly ConnectorDefinition To;

        /// <summary>
        /// Gets the defined properties.
        /// </summary>
        /// <value>The properties.</value>
        public IEnumerable<DrawingProperty> Properties { get { return _joinProperties.Values ; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="JoinDefinition" /> class.
        /// </summary>
        /// <param name="from">From connector definition.</param>
        /// <param name="to">To connector definition.</param>
        public JoinDefinition(ConnectorDefinition from, ConnectorDefinition to)
        {
            From = from;
            To = to;
        }

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <param name="property">The property.</param>
        public void AddProperty(DrawingProperty property)
        {
            _joinProperties.Add(property.Name,property);
        }
    }
}
