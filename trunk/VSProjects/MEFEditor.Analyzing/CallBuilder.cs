using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing.Editing;
using MEFEditor.Analyzing.Execution.Instructions;

namespace MEFEditor.Analyzing
{
    /// <summary>
    /// Builder of instruction info of call instructions.
    /// </summary>
    public class CallBuilder
    {
        /// <summary>
        /// The _call
        /// </summary>
        Call _call;
        /// <summary>
        /// Initializes a new instance of the <see cref="CallBuilder"/> class.
        /// </summary>
        /// <param name="call">The call.</param>
        internal CallBuilder(Call call)
        {
            _call = call;
            _call.TransformProvider = new EmptyCallTransformProvider();
        }

        /// <summary>
        /// Sets the transformation provider.
        /// </summary>
        /// <param name="transformProvider">The transform provider.</param>
        public void SetTransformationProvider(CallTransformProvider transformProvider)
        {
            _call.TransformProvider = transformProvider;
        }
    }
}
