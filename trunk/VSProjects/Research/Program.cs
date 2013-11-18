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
using AssemblyProviders.CSharp.Compiling;


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
            //var entry2 = ResearchSources.Fibonacci(7).GetResult().EntryContext;

            var assembly = ResearchSources.DrawingTester_TwoContainers();
            var executor = new AnalyzingResearchExecutor(assembly);
            executor.Execute();

            executor.TryShowDrawings();

            Console.ReadKey();
        }
    }


}

