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



using AssemblyProviders;
using AssemblyProviders.CIL;

namespace TestConsole
{
    class Program
    {
        /// <summary>
        /// Main for running research sources
        /// </summary>        
        public static void Main()
        {
            var testAssembly = TestCases.DrawingTester_SingleJoin();
            DisplayTestResult(testAssembly);
        }

        /// <summary>
        /// Run test that is defined by given assembly
        /// </summary>
        /// <param name="assembly">Assembly where test </param>
        public static void DisplayTestResult(TestingAssembly assembly)
        {
            var executor = new AnalyzingResearchExecutor(assembly);

            //execute test and show result on console and editor interface
            executor.Execute();
            executor.TryShowDrawings();

            //Wait so user can read an output
            Console.ReadKey();
        }
    }
}

