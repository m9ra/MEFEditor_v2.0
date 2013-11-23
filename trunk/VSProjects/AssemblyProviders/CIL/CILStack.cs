using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

namespace AssemblyProviders.CIL
{
    public class CILStack
    {
        private readonly Stack<Instance> _stack = new Stack<Instance>();

        public void Push(Instance pushed)
        {
            _stack.Push(pushed);
        }

        public Instance Pop()
        {
            var popped = _stack.Pop();
            return popped;
        }
    }
}
