using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MEFEditor.TypeSystem;
using MEFEditor.TypeSystem.Runtime;

namespace MEFEditor.UnitTesting.RuntimeTypeDefinitions
{
    /// <summary>
    /// Type definition used for testing purposes.
    /// </summary>
    public class MetaInterface : DataTypeDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetaInterface" /> class.
        /// </summary>
        public MetaInterface()
        {
            FullName = "MetaInterface";
        }

        /// <summary>
        /// Member representation.
        /// </summary>
        /// <returns>System.String[].</returns>
        public string[] _get_Key1()
        {
            return new[] { "Interface method cannot be called" };
        }
    }
}
