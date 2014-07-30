using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing.Editing;

namespace MEFEditor.Analyzing.Execution.Instructions
{
    /// <summary>
    /// Call according given <see cref="MethodID"/> instruction.
    /// </summary>
    class Call : InstructionBase
    {
        /// <summary>
        /// The called method.
        /// </summary>
        private readonly MethodID _method;

        /// <summary>
        /// The call arguments.
        /// </summary>
        private readonly Arguments _arguments;

        /// <summary>
        /// Gets or sets the transformation provider fo call.
        /// </summary>
        /// <value>The transformation provider.</value>
        internal CallTransformProvider TransformProvider { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Call" /> class.
        /// </summary>
        /// <param name="methodGeneratorName">Name of the method generator.</param>
        /// <param name="arguments">The arguments.</param>
        internal Call(MethodID methodGeneratorName, Arguments arguments)
        {
            _method = methodGeneratorName;
            _arguments = arguments;
        }

        /// <summary>
        /// Executes instruction in given context.
        /// </summary>
        /// <param name="context">Context where instruction is executed.</param>
        public override void Execute(AnalyzingContext context)
        {
            var argumentValues = context.GetArguments(_arguments);
           
            context.FetchCall(_method,argumentValues);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("prepare_call {0}\ncall {1}", _arguments, _method);
        }
    }
}
