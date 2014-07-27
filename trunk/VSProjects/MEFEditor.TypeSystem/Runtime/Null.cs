using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MEFEditor.TypeSystem.Runtime
{
    /// <summary>
    /// Representation of null value
    /// </summary>
    public class Null
    {
        /// <summary>
        /// Type of null representation
        /// </summary>
        public readonly static TypeDescriptor TypeInfo = TypeDescriptor.Create<Null>();
    }
}
