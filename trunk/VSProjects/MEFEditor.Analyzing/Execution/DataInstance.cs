using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing.Execution
{
    /// <summary>
    /// Implementation of <see cref="Instance"/> that can store <see cref="object"/> values in indexed fields.    
    /// </summary>
    public class DataInstance : Instance
    {
        /// <summary>
        /// The stored fields
        /// </summary>
        private readonly Dictionary<string, object> _fields = new Dictionary<string, object>();

        /// <summary>
        /// <see cref="DataInstance"/> cannot be represented as DirectValue, therefore
        /// <see cref="NotSupportedException"/> is thrown.
        /// </summary>
        /// <value>Nothing</value>
        /// <exception cref="System.NotSupportedException">Only direct instances have direct value</exception>
        public override object DirectValue
        {
            get { throw new NotSupportedException("Only direct instances have direct value"); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataInstance"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        internal DataInstance(InstanceInfo info)
            : base(info)
        {
        }

        /// <summary>
        /// Sets value for given field.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="value">The value.</param>
        public void SetField(string fieldName, object value)
        {
            _fields[fieldName] = value;
        }

        /// <summary>
        /// Gets the value of the field.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>Field's value if available, <c>null</c> otherwise.</returns>
        public object GetField(string fieldName)
        {
            object result;
            _fields.TryGetValue(fieldName, out result);
            return result;  
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("[Data]{0}", Info.TypeName);
        }
    }
}
