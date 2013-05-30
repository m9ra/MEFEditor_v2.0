using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
namespace TypeSystem.Reflection.ILAnalyzer
{
    static class ILUtilities
    {
        public static void Print(MethodInfo method)
        {
            var reader = new ILReader(method);
            Console.WriteLine(method);
            foreach (var instr in reader.Instructions)
            {
                Console.WriteLine(instr);
            }
        }
    }
}
