using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeSystem;

using AssemblyProviders.CSharp.Interfaces;

namespace AssemblyProviders.CSharp.Compiling
{
    /// <summary>
    /// Represents activation of call accepting given arguments
    /// </summary>
    class CallActivation
    {
        /// <summary>
        /// Arguments available for current activation
        /// </summary>
        private readonly List<RValueProvider> _arguments = new List<RValueProvider>();

        /// <summary>
        /// Method which call is represented by current activation
        /// </summary>
        internal readonly TypeMethodInfo MethodInfo;

        /// <summary>
        /// Node representing call corresponding to current activation
        /// </summary>
        internal INodeAST CallNode;

        /// <summary>
        /// Object which method call is represented by current activation
        /// </summary>
        internal RValueProvider CalledObject;

        /// <summary>
        /// Arguments available for current activation
        /// </summary>
        internal IEnumerable<RValueProvider> Arguments{get{return _arguments;}}


        internal CallActivation(TypeMethodInfo methodInfo)
        {
            MethodInfo = methodInfo;
        }

        /// <summary>
        /// Add call argument into current activation. 
        /// <remarks>Ordering of arguments is considered</remarks>
        /// </summary>
        /// <param name="arg">Added argument</param>
        internal void AddArgument(RValueProvider arg)
        {
            _arguments.Add(arg);
        }
    }
}
