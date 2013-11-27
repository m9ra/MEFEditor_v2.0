using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;

using Analyzing;
using Analyzing.Execution;

namespace AssemblyProviders.CIL
{
    internal delegate object MathOp(object op1, object op2);

    public class VMStack
    {
        private readonly Stack<Instance> _stack = new Stack<Instance>();

        private readonly AnalyzingContext _context;

        /// <summary>
        /// Create representation of VM stack.
        /// </summary>
        /// <param name="context">Analyzing context available for method call</param>
        private VMStack(AnalyzingContext context)
        {
            _context = context;
        }

        internal static void InitializeStack(AnalyzingContext context)
        {
            var stackInstance = context.Machine.CreateDirectInstance(new VMStack(context));
            context.SetValue(new VariableName(Transcription.StackStorage), stackInstance);
        }

        public void Push(Instance pushed)
        {
            _stack.Push(pushed);
        }

        public Instance Pop()
        {
            var popped = _stack.Pop();
            return popped;
        }

        /// <summary>
        /// Add two values on top of the stack, pop them and push the result.
        /// </summary>
        public void Add()
        {
            var op2 = Pop().DirectValue;
            var op1 = Pop().DirectValue;

            //TODO proper adding
            var res = (int)op1 + (int)op2;

            _stack.Push(createInstance(res));
        }

        /// <summary>
        /// Pop two values on top of the stack, compare them and push the result.
        /// </summary>
        public void CLT()
        {
            var op2 = Pop().DirectValue as IComparable;
            var op1 = Pop().DirectValue as IComparable;

            var isLesser = op1.CompareTo(op2) == -1;

            var pushed = isLesser ? 1 : 0;
            _stack.Push(createInstance(pushed));
        }

        private Instance createInstance(object obj)
        {
            return _context.Machine.CreateDirectInstance(obj);
        }

        public override string ToString()
        {
            return string.Format("Stack state: {0}", _stack.Count);
        }
    }
}
