using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using Analyzing;
using Analyzing.Execution;

using TypeSystem;

namespace AssemblyProviders.CIL
{
    /// <summary>
    /// Compiler used for CIL transcription. Needs VMStack as direct type in machine settings.
    /// </summary>
    public class Compiler
    {
        /// <summary>
        /// Compiled method
        /// </summary>
        private readonly CILMethod _method;

        /// <summary>
        /// Info of compiled method
        /// </summary>
        private readonly TypeMethodInfo _methodInfo;

        /// <summary>
        /// Transcriptor used for compilation
        /// </summary>
        private readonly EmitterBase E;


        public static void GenerateInstructions(CILMethod method, TypeMethodInfo info, EmitterBase emitter, TypeServices services)
        {
            Console.WriteLine(method.ToString());

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
        /// Generate instructions of _method
        /// </summary>
        private void generateInstructions()
        {
            compilerPreparations();

            Transcription.Transcript(_methodInfo, _method.Instructions, E);
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

        /// <summary>
        /// Prepare stack object
        /// </summary>
        private void prepareStack()
        {
            E.DirectInvoke(VMStack.InitializeStack);
        }

    }
}
