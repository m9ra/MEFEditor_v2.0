using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeSystem;

namespace AssemblyProviders.ProjectAssembly
{
    /// <summary>
    /// Activation describing method that could be parsed
    /// </summary>
    public class ParsingActivation
    {
        /// <summary>
        /// Source code of parsed method
        /// </summary>
        internal readonly string SourceCode;

        /// <summary>
        /// <see cref="TypeMethodInfo"/> describing generated method
        /// </summary>
        internal readonly TypeMethodInfo Method;

        /// <summary>
        /// <see cref="TypeMethodInfo"/> describing generated method definition
        /// </summary>
        internal readonly TypeMethodInfo MethodDefinition;


        internal ParsingActivation(string sourceCode, TypeMethodInfo method, TypeMethodInfo methodDefinition)
        {
            SourceCode = sourceCode;
            Method = method;
            MethodDefinition = methodDefinition;
        }
    }
}
