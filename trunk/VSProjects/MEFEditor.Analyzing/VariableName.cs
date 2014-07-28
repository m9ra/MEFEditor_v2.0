using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing
{
    /// <summary>
    /// Representation of variable that is used by <see cref="Machine"/>. It
    /// provides hasing services according to wrapped name. It is used because of
    /// avoiding string usage with variable usage.
    /// </summary>
    public class VariableName
    {
        /// <summary>
        /// The name of variable.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableName"/> class.
        /// </summary>
        /// <param name="name">The name of variable.</param>
        public VariableName(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            var o = obj as VariableName;
            if (o == null)
            {
                return false;
            }

            return o.Name == Name;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return "[Variable]"+Name;
        }
    }
}
