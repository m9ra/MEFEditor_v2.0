using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using MEFEditor.Analyzing;
using MEFEditor.Analyzing.Execution;

using MEFEditor.TypeSystem;

namespace RecommendedExtensions.Core.Languages.CIL
{
    /// <summary>
    /// Compiler used for CIL transcription. Needs <see cref="VMStack"/> as direct type in machine settings.
    /// </summary>
    public class Compiler
    {
        /// <summary>
        /// Compiled method.
        /// </summary>
        private readonly CILMethod _method;

        /// <summary>
        /// Info of compiled method.
        /// </summary>
        private readonly TypeMethodInfo _methodInfo;

        /// <summary>
        /// Transcriptor used for compilation.
        /// </summary>
        private readonly EmitterBase E;
        
        /// <summary>
        /// Generates the instructions of given method.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="info">The method information.</param>
        /// <param name="emitter">The emitter where instructions will be generated.</param>
        /// <param name="services">The services from <see cref="MEFEditor.TypeSystem"/>.</param>
        public static void GenerateInstructions(CILMethod method, TypeMethodInfo info, EmitterBase emitter, TypeServices services)
        {
            Console.WriteLine(method.ToString());

            var compiler = new Compiler(method, info, emitter, services);
            compiler.generateInstructions();

            //Console.WriteLine(emitter.GetEmittedInstructions().Code);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Compiler"/> class.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="methodInfo">The method information.</param>
        /// <param name="emitter">The emitter.</param>
        /// <param name="services">The services.</param>
        private Compiler(CILMethod method, TypeMethodInfo methodInfo, EmitterBase emitter, TypeServices services)
        {
            _method = method;
            _methodInfo = methodInfo;
            E = emitter;
        }

        /// <summary>
        /// Generate instructions of _method.
        /// </summary>
        private void generateInstructions()
        {
            compilerPreparations();

            Transcription.Transcript(_methodInfo, _method.Instructions, E);
        }

        /// <summary>
        /// Generate header with instructions providing compiler environment.
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
        /// Prepare stack object.
        /// </summary>
        private void prepareStack()
        {
            E.DirectInvoke(VMStack.InitializeStack);
        }

    }
}
