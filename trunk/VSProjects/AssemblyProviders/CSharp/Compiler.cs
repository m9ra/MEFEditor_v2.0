using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using TypeSystem;
using AssemblyProviders.CSharp.Primitives;

namespace AssemblyProviders.CSharp
{
    public class Compiler
    {
        private readonly CodeNode _node;
        public Compiler(CodeNode node)
        {
            _node = node;
        }
        
        public void GenerateInstructions(IEmitter<MethodID, InstanceInfo> emitter)
        {
            throw new NotImplementedException();            
        }
    }
}
