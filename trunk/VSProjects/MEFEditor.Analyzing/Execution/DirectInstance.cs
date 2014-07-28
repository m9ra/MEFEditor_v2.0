using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing.Editing;

namespace MEFEditor.Analyzing.Execution
{
    /// <summary>
    /// Implementation of <see cref="Instance" /> that represents direct (native .NET) <see cref="object" /> values.
    /// </summary>
    public class DirectInstance : Instance
    {
        /// <summary>
        /// The represented direct (native .NET) object.
        /// </summary>
        private object _directValue;

        /// <summary>
        /// The machine that has created current instance.
        /// </summary>
        private readonly Machine _machine;

        /// <summary>
        /// The represented direct (native .NET) object.
        /// </summary>
        /// <value>The direct value.</value>
        public override object DirectValue { get { return _directValue; } }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectInstance"/> class.
        /// </summary>
        /// <param name="directValue">The direct value.</param>
        /// <param name="info">Information stored with current <see cref="DirectInstance"/>.</param>
        /// <param name="creatingMachine">The creating machine.</param>
        internal DirectInstance(object directValue, InstanceInfo info, Machine creatingMachine)
            : base(info)
        {
            _directValue = directValue;
            _machine = creatingMachine;
        }

        /// <summary>
        /// Initializes the specified direct data.
        /// </summary>
        /// <param name="data">The direct data.</param>
        internal void Initialize(object data)
        {
            _directValue = data;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            if (DirectValue != null)
            {
                return string.Format("[{0}]{1}", Info.TypeName, DirectValue.ToString());
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
