using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing
{
    /// <summary>
    /// Abstract class that provides ability to hold information about concrete <see cref="Instance"/> objects.
    /// Implementation of <see cref="InstanceInfo"/> has to be assigned to every
    /// created <see cref="Instance"/>. 
    /// </summary>
    public abstract class InstanceInfo
    {
        /// <summary>
        /// Type representation of <see cref="Instance"/>. 
        /// <remarks>This is not required by <see cref="Machine"/>, however higher 
        /// level abstraction can utilize this info.</remarks>
        /// </summary>
        public readonly string TypeName;

        /// <summary>
        /// Gets the hint for default <see cref="Instance"/>identifier.
        /// </summary>
        /// <value>The default identifier hint.</value>
        public abstract string DefaultIdHint { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstanceInfo" /> class.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <exception cref="System.NotSupportedException">Unsupported typename:  + typeName</exception>
        public InstanceInfo(string typeName)
        {
            if (typeName == null || typeName == "")
            {
                throw new NotSupportedException("Unsupported typename: " + typeName);
            }
            TypeName = typeName;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return "[InstanceInfo]" + TypeName;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return TypeName.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            var o = obj as InstanceInfo;
            if (o == null)
            {
                return false;
            }
            return TypeName.Equals(o.TypeName);
        }
    }
}
