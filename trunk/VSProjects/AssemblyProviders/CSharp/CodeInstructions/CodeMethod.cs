using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AssemblyProviders.CSharp.Interfaces;

namespace AssemblyProviders.CSharp.CodeInstructions
{
    class CodeMethod:ICodeMethod
    {
        public CodeMethod(IEnumerable<IVariableInfo> locals, ICodeInstruction firstInstruction)
        {
            this.Locals = locals.ToArray();   
            this.FirstInstruction = firstInstruction;
        }          
        public IVariableInfo[] Locals { get; private set; }
        public ICodeInstruction FirstInstruction { get; private set; }
    }
}
