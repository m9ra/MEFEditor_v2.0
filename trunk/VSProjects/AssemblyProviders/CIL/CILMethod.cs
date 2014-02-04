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
    public class CILMethod
    {
        public readonly IEnumerable<CILInstruction> Instructions;

        public readonly string Name;

        public readonly string MethodHeader;

        public CILMethod(MethodInfo method)
        {
            Name = method.Name;
            MethodHeader = method.ToString();
            var reader = new ILReader(method);

            Instructions = from instruction in reader.Instructions select new CILInstruction(instruction);
        }

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
           

        public override string ToString()
        {
            var result = new StringBuilder();
            result.AppendLine(MethodHeader);
            foreach (var instruction in Instructions)
            {
                result.AppendLine(instruction.ToString());
            }

            return result.ToString();
        }
    }
}
