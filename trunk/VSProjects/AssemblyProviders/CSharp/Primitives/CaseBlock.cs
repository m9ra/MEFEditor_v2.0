using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AssemblyProviders.CSharp.Interfaces;

namespace AssemblyProviders.CSharp.Primitives
{
    class CaseBlock
    {
        /// <summary>
        /// Value which determine if FirstInstruction has to be executed
        /// NOTE:
        ///     null for default
        /// </summary>
        public readonly ICodeValueProvider Value;
        /// <summary>
        /// First instruction of block
        /// </summary>
        public readonly ICodeInstruction FirstInstruction;

        public CaseBlock(ICodeValueProvider value, ICodeInstruction firstInstruction)
        {
            Value = value;
            FirstInstruction=firstInstruction;
        }
    }
}
