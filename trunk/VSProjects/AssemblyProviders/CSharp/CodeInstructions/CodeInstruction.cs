using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AssemblyProviders.CSharp.Interfaces;

namespace AssemblyProviders.CSharp.CodeInstructions
{
    class CodeInstruction:ICodeInstruction
    {
        public CodeInstruction(ICodeStatement statement)
        {
            this.Statement = statement;
        }
        public ICodeInstruction Next { get;set; }
        public ICodeStatement Statement { get; private set; }
    }
}
