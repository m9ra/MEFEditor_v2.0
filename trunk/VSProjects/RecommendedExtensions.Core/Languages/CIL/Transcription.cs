using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using R = System.Reflection;

using Mono.Cecil;

using MEFEditor.Analyzing;
using MEFEditor.TypeSystem;
using MEFEditor.TypeSystem.Runtime;

using RecommendedExtensions.Core.Services;
using RecommendedExtensions.Core.Languages.CIL.ILAnalyzer;

namespace RecommendedExtensions.Core.Languages.CIL
{
    /// <summary>
    /// Class providing transcription services for CIL instructions
    /// Transcription is provided by transcriptors that are defined as private methods
    /// with signature: _transcriptor()
    /// All these methods are fetched in static initializer and used ILInstruction transcription
    /// 
    /// Transcriptor is used for opcode with longest matching name beginning.
    /// </summary>
    static class Transcription
    {
        /// <summary>
        /// Storage variable where stack is stored
        /// </summary>
        internal static readonly string StackStorage = "@stack";

        /// <summary>
        /// Constructor method of stack
        /// </summary>
        internal static readonly MethodID Stack_ctor = Naming.Method<VMStack>(Naming.CtorName);

        /// <summary>
        /// Push method of stack
        /// </summary>
        internal static readonly MethodID Stack_push = Naming.Method<VMStack>("Push", typeof(object));

        /// <summary>
        /// Pop method of stack
        /// </summary>
        internal static readonly MethodID Stack_pop = Naming.Method<VMStack>("Pop");

        /// <summary>
        /// Fake method of stack
        /// </summary>
        internal static readonly MethodID Stack_fakeInstruction = Naming.Method<VMStack>("Fake");

        /// <summary>
        /// Pop array size and Push new array on the stack
        /// </summary>
        internal static readonly MethodID Stack_newarr = Naming.Method<VMStack>("NewArr");

        /// <summary>
        /// Pop array size and Push new array on the stack
        /// </summary>
        internal static readonly MethodID Stack_stelem = Naming.Method<VMStack>("StElem");

        /// <summary>
        /// Load the element at index onto the top of the stack.
        /// </summary>
        internal static readonly MethodID Stack_ldelem = Naming.Method<VMStack>("LdElem");

        /// <summary>
        /// Duplicate the value on the top of the stack.
        /// </summary>
        internal static readonly MethodID Stack_dup = Naming.Method<VMStack>("Dup");

        /// <summary>
        /// Add two operands on top of the stack and push its result
        /// </summary>
        internal static readonly MethodID Stack_add = Naming.Method<VMStack>("Add");

        /// <summary>
        /// Pop two values on top of the stack, compare them and push the result.
        /// </summary>
        internal static readonly MethodID Stack_clt = Naming.Method<VMStack>("CLT");

        /// <summary>
        /// System object info
        /// </summary>
        internal static readonly InstanceInfo Object_info = TypeDescriptor.Create<object>();

        /// <summary>
        /// System void info
        /// </summary>
        internal static readonly InstanceInfo Void_info = TypeDescriptor.Void;

        /// <summary>
        /// Table of instruction transcriptors
        /// </summary>
        internal static readonly Dictionary<string, Action> _transcriptors = new Dictionary<string, Action>();

        #region Data for instruction transcription

        /// <summary>
        /// Transcripted instruction
        /// </summary>
        private static CILInstruction Instruction;

        /// <summary>
        /// Emitter available for transcription
        /// </summary>
        private static EmitterBase E;

        /// <summary>
        /// Context method of transcription
        /// </summary>
        private static TypeMethodInfo Method;

        /// <summary>
        /// Offset to corresponding label tabel
        /// </summary>
        private static readonly Dictionary<int, MEFEditor.Analyzing.Label> Labels = new Dictionary<int, MEFEditor.Analyzing.Label>();

        /// <summary>
        /// Variable that could be used for storing info locally in within transcriptor
        /// Is of object type
        /// </summary>
        private static string LocalTmpVar;

        /// <summary>
        /// Determine that trancripted method has return value
        /// </summary>
        private static bool HasReturnValue { get { return !Method.ReturnType.Equals(TypeDescriptor.Void); } }

        /// <summary>
        /// Data of transcripted instruction
        /// </summary>
        private static object Data { get { return Instruction.Data; } }

        /// <summary>
        /// Name of transcripted instruction
        /// </summary>
        private static string Name { get { return Instruction.OpCode.Name; } }

        #endregion

        /// <summary>
        /// Initialize transcription table
        /// </summary>
        static Transcription()
        {
            var methods = typeof(Transcription).GetMethods(R.BindingFlags.Static | R.BindingFlags.NonPublic);
            var transcriptors = (from method in methods where method.Name.StartsWith("_") select method).ToArray();

            var transcriptorNaming = new SortedList<string, Action>();
            foreach (var transcriptor in transcriptors)
            {
                //translate name according to transcriptors convention
                var name = transcriptor.Name.Substring(1).Replace("_", ".");

                //This requires .NET4.5
                //var handler = (Action)transcriptor.CreateDelegate(typeof(Action));

                var call = Expression.Call(null, transcriptor);
                var handler = Expression.Lambda<Action>(call).Compile();
                transcriptorNaming.Add(name, handler);
            }

            //register all opcodes
            foreach (var opcode in ILReader.Opcodes)
            {
                //try match with every available transcriptor
                foreach (var namedTranscriptor in transcriptorNaming.Reverse())
                {
                    if (opcode.Name.StartsWith(namedTranscriptor.Key))
                    {
                        _transcriptors.Add(opcode.Name, namedTranscriptor.Value);
                        Console.WriteLine(opcode.Name);
                        break;
                    }
                }
            }
        }


        /// <summary>
        /// Transcript given instructions in context of given method.
        /// Transcription is processed by given emitter.
        /// </summary>
        /// <param name="method">Context method of transcription</param>
        /// <param name="instructions">Transcripted instructions</param>
        /// <param name="emitter">Emitter where transcription is emitted</param>
        internal static void Transcript(TypeMethodInfo method, IEnumerable<CILInstruction> instructions, EmitterBase emitter)
        {
            E = emitter;
            Method = method;
            LocalTmpVar = E.GetTemporaryVariable("local");

            //prepare labels table
            Labels.Clear();
            foreach (var instruction in instructions)
            {
                var labelName = string.Format("L_0x{0:x4}", instruction.Address);
                Labels.Add(instruction.Address, E.CreateLabel(labelName));
            }

            foreach (var instruction in instructions)
            {
                Instruction = instruction;
                E.SetLabel(Labels[instruction.Address]);

                var block = E.StartNewInfoBlock();
                block.Comment = "\n---" + Instruction.ToString();

                Action transcriptor;
                if (_transcriptors.TryGetValue(Name, out transcriptor))
                {
                    transcriptor();
                }
                else
                {
                    unknownInstruction();
                }
            }
        }


        #region Transctiptors definitions

        /// <summary>
        /// Transcriptor for instruction without defined transcriptor
        /// </summary>
        static void unknownInstruction()
        {
            emitPush<CILInstruction>(Instruction);
            stackCall(Stack_fakeInstruction);
        }

        static void _nop()
        {
            E.Nop();
        }

        static void _pop()
        {
            emitPop();
        }

        static void _stloc()
        {
            emitPopTo(getLocalVar(Name));
        }

        static void _stloca()
        {
            unknownInstruction();
        }

        static void _ldloc()
        {
            emitPushFrom(getLocalVar(Name));
        }

        static void _ldloca()
        {
            unknownInstruction();
        }

        static void _ldarg()
        {
            var argNumber = getArgNumber(Name);

            emitPushArg(argNumber);
        }

        static void _ldarg_s()
        {
            var number = Instruction.Data as ParameterReference;
            if (number == null)
                unknownInstruction();
            
            emitPushArg(number.Index);
        }

        static void _ldarga()
        {
            unknownInstruction();
        }

        static void _newobj()
        {
            emitCtor(Instruction.MethodOperand);
        }

        static void _call()
        {
            var info = Instruction.MethodOperand;
            var staticCall = info.IsStatic;
            emitCall(info, staticCall);
        }

        static void _stsfld()
        {
            emitCall(Instruction.SetterOperand, true);
        }

        static void _stsflda()
        {
            unknownInstruction();
        }

        static void _ldsfld()
        {
            emitCall(Instruction.GetterOperand, true);
        }

        static void _ldsflda()
        {
            unknownInstruction();
        }

        static void _box()
        {
            //boxing is not needed
            E.Nop();
        }

        static void _ret()
        {
            if (HasReturnValue)
            {
                emitPopTo(LocalTmpVar);
                E.Return(LocalTmpVar);
            }
            else
            {
                E.Nop();
            }
        }

        static void _ldtoken()
        {
            var type = Instruction.TypeOperand;
            var literalType = new LiteralType(type);
            emitPush<LiteralType>(literalType);
        }

        static void _dup()
        {
            stackCall(Stack_dup);
        }

        #region Object instructions

        static void _ldfld()
        {
            var getter = Instruction.GetterOperand;
            emitCall(getter, getter.IsStatic);
        }

        static void _ldflda()
        {
            unknownInstruction();
        }

        static void _stfld()
        {
            var setter = Instruction.SetterOperand;
            emitCall(setter, setter.IsStatic);
        }

        static void _stflda()
        {
            unknownInstruction();
        }

        #endregion

        #region Array instructions

        static void _newarr()
        {
            stackCall(Stack_newarr);
        }

        static void _stelem()
        {
            stackCall(Stack_stelem);
        }

        static void _ldelem()
        {
            stackCall(Stack_ldelem);
        }

        #endregion

        #region Arithmetic instructions

        static void _add()
        {
            stackCall(Stack_add);
        }

        static void _clt()
        {
            stackCall(Stack_clt);
        }

        #endregion

        #region Branching instructions

        static void _br_s()
        {
            var target = getTargetLabel();
            E.Jump(target);
        }

        static void _brtrue_s()
        {
            var target = getTargetLabel();
            emitPopTo(LocalTmpVar);

            E.ConditionalJump(LocalTmpVar, target);
        }

        static void _blt_s()
        {
            _clt();
            _brtrue_s();
        }

        #endregion

        #region Constant loading instructions

        static void _ldnull()
        {
            emitPush<Null>(new Null());
        }

        static void _ldstr()
        {
            emitPush<string>(Data);
        }

        static void _ldc_i4()
        {
            emitPush<int>(Data);
        }

        static void _ldc_i4_()
        {
            //small constant instruction in form of ldc.i4.[constant cipher]
            var cipher = Name[Name.Length - 1] - '0';
            int constant = (byte)cipher;

            emitPush<int>(constant);
        }

        static void _ldc_i4_m1()
        {
            emitPush<int>(-1);
        }

        static void _ldc_i4_M1()
        {
            _ldc_i4_m1();
        }

        static void _ldc_i4_s()
        {
            var data = (int)(sbyte)Data;
            emitPush<int>(data);
        }

        static void _ldc_i8()
        {
            emitPush<Int64>(Data);
        }

        static void _ldc_r4_()
        {
            emitPush<float>(Data);
        }

        static void _ldc_r8_()
        {
            emitPush<double>(Data);
        }

        #endregion

        #endregion

        #region Private helpers

        /// <summary>
        /// Get local variable name for stloc and ldloc instructions
        /// </summary>
        /// <param name="instructionName">Name instruction which storage is searched</param>
        /// <returns></returns>
        static string getLocalVar(string instructionName)
        {
            return instructionName.Substring(2);
        }

        /// <summary>
        /// Get number of argument loaded by ldarg instruction
        /// </summary>
        /// <param name="instructionName"></param>
        /// <returns></returns>
        static int getArgNumber(string instructionName)
        {
            var argOffset = Method.HasThis ? 0 : 1;
            return int.Parse(instructionName.Substring(6)) + argOffset;
        }

        static Label getTargetLabel()
        {
            var targetOffset = Instruction.BranchAddressOperand;
            var targetLabel = Labels[targetOffset];

            return targetLabel;
        }

        private static void emitCtor(TypeMethodInfo ctor)
        {
            var arguments = emitPopArguments(ctor);

            E.AssignNewObject(LocalTmpVar, ctor.DeclaringType);
            E.Call(ctor.MethodID, LocalTmpVar, arguments);
            emitPushFrom(LocalTmpVar);
        }

        private static void emitCall(TypeMethodInfo info, bool staticCall)
        {
            var methodID = info.MethodID;

            var arguments = emitPopArguments(info);

            if (staticCall)
            {
                E.StaticCall(info.DeclaringType, methodID, arguments);
            }
            else
            {
                var calledObj = emitPopTmp(info.DeclaringType);
                E.Call(methodID, calledObj, arguments);
            }

            if (!info.ReturnType.Equals(Void_info))
            {
                emitPushReturn(info.ReturnType);
            }
        }

        private static Arguments emitPopArguments(TypeMethodInfo info)
        {
            var argumentVariables = from param in info.Parameters select emitPopTmp(param.Type);
            argumentVariables = argumentVariables.Reverse();
            var arguments = Arguments.Values(argumentVariables.ToArray());
            return arguments;
        }


        #endregion

        #region Stack operations

        private static void emitPop()
        {
            E.Call(Stack_pop, StackStorage, Arguments.Values());
        }

        private static void emitPopTo(string target)
        {
            emitPop();
            E.AssignReturnValue(target, Object_info);
        }

        private static string emitPopTmp(InstanceInfo type)
        {
            emitPop();

            var tmp = E.GetTemporaryVariable();
            E.AssignReturnValue(tmp, type);
            return tmp;
        }

        private static void emitPush<T>(object literal)
        {
            if ((literal != null) && !(literal is T))
                throw new NotSupportedException("Wrong literal pushing");

            var tmp = LocalTmpVar;
            E.AssignLiteral(tmp, literal, TypeDescriptor.Create<T>());
            emitPushFrom(tmp);
        }

        private static void emitPushReturn(InstanceInfo returnType)
        {
            var tmp = LocalTmpVar;
            E.AssignReturnValue(tmp, returnType);
            emitPushFrom(tmp);
        }

        private static void emitPushFrom(string source)
        {
            E.Call(Stack_push, StackStorage, Arguments.Values(source));
        }

        private static void emitPushArg(int argIndex)
        {
            var tmp = LocalTmpVar;
            E.AssignArgument(tmp, Object_info, (uint)argIndex);
            emitPushFrom(tmp);
        }

        private static void stackCall(MethodID method)
        {
            E.Call(method, StackStorage, Arguments.Values());
        }

        #endregion
    }
}
