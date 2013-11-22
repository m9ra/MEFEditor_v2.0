using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using AssemblyProviders.CIL.ILAnalyzer;

namespace AssemblyProviders.CIL
{
    public class CILMethod
    {
        public readonly IEnumerable<ILInstruction> Instructions;

        public readonly string Name;

        public readonly string MethodHeader;

        public CILMethod(MethodInfo method)
        {
            Name = method.Name;
            MethodHeader = method.ToString();
            var reader = new ILReader(method);
            Instructions = reader.Instructions.ToArray();
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
