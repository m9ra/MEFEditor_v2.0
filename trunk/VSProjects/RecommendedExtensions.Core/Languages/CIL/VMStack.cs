using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;

using MEFEditor.Analyzing;
using MEFEditor.Analyzing.Execution;

using Mono.Cecil.Cil;

using MEFEditor.TypeSystem;
using MEFEditor.TypeSystem.Runtime;

namespace RecommendedExtensions.Core.Languages.CIL
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
            if (_stack.Count == 0)
                throw new ParsingException("No value on stack is present because CIL transcription created incorrect program.", null);

            var popped = _stack.Pop();
            return popped;
        }

        public void Fake()
        {
            var toFake = Pop().DirectValue as CILInstruction;
            var opCode = toFake.OpCode;

            for (var i = 0; i < getPopDelta(toFake, _stack.Count); ++i)
            {
                var popped = Pop();
                _context.SetDirty(popped);
            }

            for (var i = 0; i < getPushDelta(toFake); ++i)
            {
                var fakeData=_context.Machine.CreateDirectInstance("Faking: "+toFake);
                _context.SetDirty(fakeData);
                Push(fakeData);
            }
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

        /// <summary>
        /// Duplicate the value on the top of the stack.
        /// </summary>
        public void Dup()
        {
            var duplicated = _stack.Peek();
            Push(duplicated);
        }

        /// <summary>
        /// Pop array size from the stack and push new array on the stack
        /// </summary>
        public void NewArr()
        {
            var size = (int)Pop().DirectValue;
            var array = new MEFEditor.TypeSystem.Runtime.Array<InstanceWrap>(size);

            var arrayInstance = _context.Machine.CreateDirectInstance(array);
            Push(arrayInstance);
        }

        /// <summary>
        /// Replace array element at index with the value on the stack
        /// </summary>
        public void StElem()
        {
            var element = Pop();
            var index = (int)Pop().DirectValue;
            var array = Pop().DirectValue as Array<InstanceWrap>;
            array.set_Item(index, new InstanceWrap(element));
        }

        /// <summary>
        /// Load the element at index onto the top of the stack.
        /// </summary>
        public void LdElem()
        {
            var index = (int)Pop().DirectValue;
            var array = Pop().DirectValue as Array<InstanceWrap>;

            var result = array.get_Item(index);
            Push(result.Wrapped);
        }

        private Instance createInstance(object obj)
        {
            return _context.Machine.CreateDirectInstance(obj);
        }

        #region Counting stack delta

        /// <summary>
        /// Compute delta of pushing according to given instruction
        /// <remarks>Modified method taken from http://cecil.googlecode.com/svn/trunk/decompiler/Cecil.Decompiler/Cecil.Decompiler.Cil/ControlFlowGraphBuilder.cs </remarks>
        /// </summary>
        /// <param name="instruction">Instruction which delta is computed</param>
        /// <returns>Push delta</returns>
        int getPushDelta(CILInstruction instruction)
        {
            var stackBehaviour = instruction.OpCode.StackBehaviourPush;
            switch (stackBehaviour)
            {
                case StackBehaviour.Push0:
                    return 0;

                case StackBehaviour.Push1:
                case StackBehaviour.Pushi:
                case StackBehaviour.Pushi8:
                case StackBehaviour.Pushr4:
                case StackBehaviour.Pushr8:
                case StackBehaviour.Pushref:
                    return 1;

                case StackBehaviour.Push1_push1:
                    return 2;

                case StackBehaviour.Varpush:
                    if (instruction.MethodOperand == null)
                        //cannot be determined
                        return 0;

                    return instruction.MethodOperand.ReturnType.TypeName == typeof(void).FullName ? 0 : 1;

                default:
                    //unknown behaviour
                    return 0;
            }
        }


        /// <summary>
        /// Compute delta of popping according to given instruction
        /// <remarks>Modified method taken from http://cecil.googlecode.com/svn/trunk/decompiler/Cecil.Decompiler/Cecil.Decompiler.Cil/ControlFlowGraphBuilder.cs </remarks>
        /// </summary>
        /// <param name="instruction">Instruction which delta is computed</param>
        /// <param name="stackHeight">Current height of stack that should be popped</param>
        /// <returns>Pop delta</returns>
        int getPopDelta(CILInstruction instruction, int stackHeight)
        {
            var stackBehaviour = instruction.OpCode.StackBehaviourPop;
            switch (stackBehaviour)
            {
                case StackBehaviour.Pop0:
                    return 0;
                case StackBehaviour.Popi:
                case StackBehaviour.Popref:
                case StackBehaviour.Pop1:
                    return 1;

                case StackBehaviour.Pop1_pop1:
                case StackBehaviour.Popi_pop1:
                case StackBehaviour.Popi_popi:
                case StackBehaviour.Popi_popi8:
                case StackBehaviour.Popi_popr4:
                case StackBehaviour.Popi_popr8:
                case StackBehaviour.Popref_pop1:
                case StackBehaviour.Popref_popi:
                    return 2;

                case StackBehaviour.Popi_popi_popi:
                case StackBehaviour.Popref_popi_popi:
                case StackBehaviour.Popref_popi_popi8:
                case StackBehaviour.Popref_popi_popr4:
                case StackBehaviour.Popref_popi_popr8:
                case StackBehaviour.Popref_popi_popref:
                    return 3;

                case StackBehaviour.PopAll:
                    return stackHeight;

                case StackBehaviour.Varpop:
                    if (instruction.MethodOperand != null)
                    {
                        var method = instruction.MethodOperand;
                        int count = method.Parameters.Length;
                        if (method.HasThis && OpCodes.Newobj.Value != instruction.OpCode.Value)
                            ++count;

                        return count;
                    }

                    //if (instruction.OpCode.Code == Code.Ret)
                    //return IsVoidMethod() ? 0 : 1;
                    return 0; //we dont now if we are in void method


            }

            return 0;
        }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("Stack state: {0}", _stack.Count);
        }
    }
}
