using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Reflection.Emit;

using Analyzing;
using TypeSystem;

using AssemblyProviders.CIL.ILAnalyzer;

namespace AssemblyProviders.CIL
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
        internal static readonly MethodID Stack_ctor = Naming.Method<CILStack>(Naming.CtorName);

        /// <summary>
        /// Push method of stack
        /// </summary>
        internal static readonly MethodID Stack_push = Naming.Method<CILStack>("Push", typeof(object));

        /// <summary>
        /// Pop method of stack
        /// </summary>
        internal static readonly MethodID Stack_pop = Naming.Method<CILStack>("Pop");

        /// <summary>
        /// System object info
        /// </summary>
        internal static readonly InstanceInfo Object_info = InstanceInfo.Create<object>();

        /// <summary>
        /// System void info
        /// </summary>
        internal static readonly InstanceInfo Void_info = new InstanceInfo(typeof(void));

        /// <summary>
        /// Table of instruction transcriptors
        /// </summary>
        internal static readonly Dictionary<string, Action> _transcriptors = new Dictionary<string, Action>();

        #region Data for instruction transcription

        /// <summary>
        /// Transcripted instruction
        /// </summary>
        private static ILInstruction Instruction;

        /// <summary>
        /// Emitter available for transcription
        /// </summary>
        private static EmitterBase E;

        /// <summary>
        /// Context method of transcription
        /// </summary>
        private static TypeMethodInfo Method;

        /// <summary>
        /// Variable that could be used for storing info locally in within transcriptor
        /// Is of object type
        /// </summary>
        private static string LocalTmpVar;

        /// <summary>
        /// Determine that trancripted method has return value
        /// </summary>
        private static bool HasReturnValue { get { return !Method.ReturnType.Equals(new InstanceInfo(typeof(void))); } }

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
            var methods = typeof(Transcription).GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
            var transcriptors = (from method in methods where method.Name.StartsWith("_") select method).ToArray();

            var transcriptorNaming = new SortedList<string, Action>();
            foreach (var transcriptor in transcriptors)
            {
                //translate name according to transcriptors convention
                var name = transcriptor.Name.Substring(1).Replace("_", ".");

                var handler = (Action)transcriptor.CreateDelegate(typeof(Action));
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
        internal static void Transcript(TypeMethodInfo method, IEnumerable<ILInstruction> instructions, EmitterBase emitter)
        {
            E = emitter;
            Method = method;
            LocalTmpVar = E.GetTemporaryVariable("local");

            foreach (var instruction in instructions)
            {
                Instruction = instruction;

                var block = E.StartNewInfoBlock();
                block.Comment = "\n---" + Instruction.ToString();

                Action transcriptor;
                if (_transcriptors.TryGetValue(Name, out transcriptor))
                {
                    transcriptor();
                }
                else
                {
                    unknwonInstruction();
                }
            }
        }


        #region Transctiptors definitions

        /// <summary>
        /// Transcriptor for instruction without defined transcriptor
        /// </summary>
        static void unknwonInstruction()
        {
            //TODO handle stack behaviour with setting dirty
            E.AssignLiteral(LocalTmpVar, "NotImplemented instruction: " + Name);
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

        static void _ldloc()
        {
            emitPushFrom(getLocalVar(Name));
        }

        static void _call()
        {
            var info = Data as MethodInfo;
            var methodID = Naming.Method(info);

            var argumentVariables = from param in info.GetParameters() select emitPopTmp(param.ParameterType);
            argumentVariables = argumentVariables.Reverse();

            var arguments = Arguments.Values(argumentVariables.ToArray());

            if (info.IsStatic)
            {
                E.StaticCall(new InstanceInfo(info.DeclaringType), methodID, arguments);
            }
            else
            {
                var calledObj = emitPopTmp(info.DeclaringType);
                E.Call(methodID, calledObj, arguments);
            }

            if (info.ReturnType != typeof(void))
            {
                emitPushReturn(info.ReturnType);
            }
        }

        static void _ret()
        {
            if (HasReturnValue)
            {
                emitPopTo(LocalTmpVar);
                E.Return(LocalTmpVar);
            }
        }

        #region Constant loading instructions

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
            var data = (int)(sbyte)(byte)Data;
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

        private static string emitPopTmp(Type type)
        {
            emitPop();

            var tmp = E.GetTemporaryVariable();
            E.AssignReturnValue(tmp, new InstanceInfo(type));
            return tmp;
        }

        private static void emitPush<T>(object literal)
        {
            if (!(literal is T))
                throw new NotSupportedException("Wrong literal pushing");

            var tmp = LocalTmpVar;
            E.AssignLiteral(tmp, literal, InstanceInfo.Create<T>());
            emitPushFrom(tmp);
        }

        private static void emitPushReturn(Type returnType)
        {
            var tmp = LocalTmpVar;
            E.AssignReturnValue(tmp, new InstanceInfo(returnType));
            emitPushFrom(tmp);
        }

        private static void emitPushFrom(string source)
        {
            E.Call(Stack_push, StackStorage, Arguments.Values(source));
        }

        #endregion
    }
}
