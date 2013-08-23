using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AssemblyProviders.CSharp.Primitives;

namespace AssemblyProviders.CSharp
{
    public class Source
    {
        public string Code { get; private set; }
        public Source(string code)
        {
            Code = code;
        }

        internal void Remove(Position p1, Position p2)
        {
            Code = Code.Substring(0,p1.Offset)+Code.Substring(p2.Offset);            
        }
    }
}
