using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Analyzing;

using AssemblyProviders.CSharp.Interfaces;

namespace AssemblyProviders.CSharp.Compiling
{
    /// <summary>
    /// Context of compiled block
    /// </summary>
    class BlockContext
    {
        /// <summary>
        /// Block which current context belongs to
        /// </summary>
        public readonly INodeAST Block;

        /// <summary>
        /// Label which is used as target for break 
        /// statement. If break cannot be used is null.
        /// </summary>
        public readonly Label BreakLabel;

        /// <summary>
        /// Label which is used as target for continue 
        /// statement. If continue cannot be used is null.
        /// </summary>
        public readonly Label ContinueLabel;

        /// <summary>
        /// Initialize new <see cref="BlockContext"/> object.
        /// </summary>
        /// <param name="block">Block which context belongs to</param>
        /// <param name="continueLabel">Label which is used as target for continue</param>
        /// <param name="breakLabel">Label which is used as target for break</param>
        internal BlockContext(INodeAST block, Label continueLabel, Label breakLabel)
        {
            Block = block;
            BreakLabel = breakLabel;
            ContinueLabel = continueLabel;
        }
    }
}
