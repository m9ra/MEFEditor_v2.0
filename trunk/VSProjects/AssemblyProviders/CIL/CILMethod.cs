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
        public CILMethod(MethodInfo method)
        {
            Name = method.Name;
            MethodDescription = method.ToString();
            var reader = new ILReader(method);

            Instructions = from instruction in reader.Instructions select new CILInstruction(instruction);
        }

        /// <summary>
        /// Create CILMethod from MethodDefinition representation. Is used for 
        /// creating methods from methods loaded by Mono.Cecil.
        /// </summary>
        /// <param name="method">Mono.Cecil method</param>
        public CILMethod(MethodDefinition method)
        {
            if (method == null)
            {
                //empty method
                Instructions = new CILInstruction[0];
            }
            else
            {
                //wrap instructions
                Instructions = from instruction in method.Body.Instructions select new CILInstruction(instruction);
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
