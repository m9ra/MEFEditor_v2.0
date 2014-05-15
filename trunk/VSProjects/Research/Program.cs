using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Diagnostics;

using TypeSystem;
using UnitTesting.Analyzing_TestUtils;
using UnitTesting.TypeSystem_TestUtils;

using Mono.Cecil;

using AssemblyProviders;
using AssemblyProviders.CIL;

namespace Research
{
    class Program
    {
        /// <summary>
        /// Main for running research sources
        /// </summary>        
        static void Main()
        {
            ////force JIT to precompile before measuring
            //var entry2 = ResearchSources.Fibonacci(7);

            var assembly = ResearchSources.Edit_SemanticEnd_CommonScope();
            var executor = new AnalyzingResearchExecutor(assembly);
            executor.Execute();

            executor.TryShowDrawings();

            Console.ReadKey();
        }
    }
}

