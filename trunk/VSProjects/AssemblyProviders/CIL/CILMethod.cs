using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Analyzing;
using TypeSystem;

using AssemblyProviders.CIL.ILAnalyzer;

namespace AssemblyProviders.CIL
{
    /// <summary>
    /// Method representation used by CILAssembly. Can handle MethodDefinition or
    /// MethodInfo. Provide methods translation into CILInstructions.
    /// </summary>
    public class CILMethod
    {
        /// <summary>
        /// Context of transcription if available
        /// </summary>
        internal readonly TranscriptionContext Context;

        /// <summary>
        /// Instructions of method.
        /// </summary>
        public readonly IEnumerable<CILInstruction> Instructions;

        /// <summary>
        /// Method name.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Description displayed in AIL comments.
        /// </summary>
        public readonly string MethodDescription;

        /// <summary>
        /// Create CILMethod from MethodInfo representation. Is used for
        /// creating methods from runtime .NET methods.
        /// </summary>
        /// <param name="method">Runtime .NET method representation</param>
        public CILMethod(MethodInfo method, TypeMethodInfo methodInfo)
        {
            //TODO: Reflection methods doesnt support now context generic parameters
            Context = new TranscriptionContext(methodInfo, new GenericParameter[0]);
            Name = method.Name;
            MethodDescription = method.ToString();
            var reader = new ILReader(method);

            Instructions = from instruction in reader.Instructions select new CILInstruction(instruction, Context);
        }

        /// <summary>
        /// Create CILMethod from MethodDefinition representation. Is used for 
        /// creating methods from methods loaded by Mono.Cecil.
        /// </summary>
        /// <param name="method">Mono.Cecil method</param>
        public CILMethod(MethodDefinition method, TypeMethodInfo methodInfo)
        {
            var genericTypeParameters = method.DeclaringType.GenericParameters;
            var genericMethodParameters = method.GenericParameters;

            Context = new TranscriptionContext(methodInfo, genericTypeParameters.Concat(genericMethodParameters));
            if (method == null)
            {
                //empty method
                Instructions = new CILInstruction[0];
            }
            else
            {
                //wrap instructions
                Instructions = from instruction in method.Body.Instructions select new CILInstruction(instruction, Context);
            }
        }

        /// <summary>
        /// Human readable description of method.
        /// </summary>
        /// <returns>Description of method.</returns>
        public override string ToString()
        {
            var result = new StringBuilder();
            result.AppendLine(MethodDescription);
            foreach (var instruction in Instructions)
            {
                result.AppendLine(instruction.ToString());
            }

            return result.ToString();
        }
    }
}
