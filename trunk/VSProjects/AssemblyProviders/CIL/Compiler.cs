using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using Analyzing.Execution;

using TypeSystem;

namespace AssemblyProviders.CIL
{
    public class Compiler
    {
        private static readonly string StackStorage = "@stack";
        private static readonly MethodID Stack_ctor = Naming.Method<CILStack>(Naming.CtorName);
        private static readonly MethodID Stack_push = Naming.Method<CILStack>("Push", typeof(object));
        private static readonly MethodID Stack_pop = Naming.Method<CILStack>("Pop");

        private static readonly InstanceInfo Object_info = InstanceInfo.Create<object>();

        private readonly CILMethod _method;
        private readonly EmitterBase E;
        private readonly TypeMethodInfo _methodInfo;

        public static void GenerateInstructions(CILMethod method, TypeMethodInfo info, EmitterBase emitter, TypeServices services)
        {
            var compiler = new Compiler(method, info, emitter, services);

            compiler.generateInstructions();

            //Console.WriteLine(emitter.GetEmittedInstructions().Code);
        }

        private Compiler(CILMethod method, TypeMethodInfo methodInfo, EmitterBase emitter, TypeServices services)
        {
            _method = method;
            _methodInfo = methodInfo;
            E = emitter;
        }

        /// <summary>
        /// Generate header with instructions providing compiler environment
        /// </summary>
        private void compilerPreparations()
        {
            E.StartNewInfoBlock().Comment = "===Compiler initialization===";

            prepareStack();

            if (_methodInfo.HasThis)
            {
                E.AssignArgument("this", _methodInfo.DeclaringType, 0);
            }

            //generate argument assigns
            for (uint i = 0; i < _methodInfo.Parameters.Length; ++i)
            {
                var arg = _methodInfo.Parameters[i];
                E.AssignArgument(arg.Name, arg.Type, i + 1); //argument 0 is always this object

                //TODO declare arguments
            }
        }

        private void prepareStack()
        {
            E.AssignNewObject(StackStorage, InstanceInfo.Create<CILStack>());
            E.Call(Stack_ctor, StackStorage, Arguments.Values());
        }

        private void emitPush<T>(object literal)
        {
            if (!(literal is T))
                throw new NotSupportedException("Wrong literal pushing");

            var tmp = E.GetTemporaryVariable("push");
            E.AssignLiteral(tmp, literal, InstanceInfo.Create<T>());
            E.Call(Stack_push, StackStorage, Arguments.Values(tmp));
        }

        private void emitPopTo(string target)
        {
            E.Call(Stack_pop, StackStorage, Arguments.Values());
            E.AssignReturnValue(target, Object_info);
        }

        private void emitPushFrom(string source)
        {
            E.Call(Stack_push, StackStorage, Arguments.Values(source));
        }

        private void generateInstructions()
        {
            compilerPreparations();

            foreach (var instruction in _method.Instructions)
            {
                var block = E.StartNewInfoBlock();
                block.Comment = "\n---" + instruction.ToString();

                var name = instruction.OpCode.Name;

                switch (name)
                {
                    case "ldstr":
                        emitPush<string>(instruction.Data);
                        break;
                    case "nop":
                        E.Nop();
                        break;
                    case "stloc.0":
                    case "stloc.1":
                        //TODO generic add store loc operands
                        emitPopTo(name.Substring(2));
                        break;
                    case "ldloc.0":
                    case "ldloc.1":
                        //TODO generic add store loc operands
                        emitPushFrom(name.Substring(2));
                        break;
                    case "ret":
                        var tmp=E.GetTemporaryVariable("return");
                        emitPopTo(tmp);
                        E.Return(tmp);
                        break;
                    default:
                        //TODO handle stack behaviour with setting dirty
                        var temp = E.GetTemporaryVariable();
                        E.AssignLiteral(temp, "NotImplemented instruction: " + name);
                        break;
                }
            }
        }
    }
}
