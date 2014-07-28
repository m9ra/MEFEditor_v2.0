using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Drawing
{
    /// <summary>
    /// Representation of named value that is used
    /// by drawing definitions.
    /// </summary>
    public class DrawingProperty
    {
        /// <summary>
        /// The name of property.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The value of property.
        /// </summary>
        public readonly string Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingProperty" /> class.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="value">The value.</param>
        public DrawingProperty(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
