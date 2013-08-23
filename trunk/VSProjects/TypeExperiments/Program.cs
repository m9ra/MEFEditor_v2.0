using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



using TypeExperiments.Core;
using TypeExperiments.TypeBuilding;
using TypeExperiments.Reflection.ILAnalyzer;

using System.Diagnostics;

using TypeSystem;
using UnitTesting.TypeSystem_TestUtils;
using AssemblyProviders.CSharp.Compiling;


namespace TypeExperiments
{
    class Program
    {

        /*   string x;
           void test()
           {            
               testRef(ref x);
           }

           void testRef(ref string x)
           {
               x = "44";
           }

           static void Main(string[] args)
           {
               Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
        //       Benchmark.WrappTypeTest();
               ILUtilities.Print(typeof(Program).GetMethod("test",BindingFlags.Instance|BindingFlags.NonPublic));
           }
           */

        /// <summary>
        /// Main for CSharp compiler developing
        /// </summary>
        static void Main()
        {
            //force JIT to precompile before measuring
            var entry2 = ResearchSources.Fibonacci(7).GetResult().EntryContext;

            var watch=Stopwatch.StartNew();
            var entry = ResearchSources.EditProvider().GetResult().EntryContext;
            watch.Stop();


            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("ENTRY CONTEXT - Variable values");
            PrinterIAL.PrintVariables(entry);

            Console.ForegroundColor = ConsoleColor.Red;            
            Console.WriteLine("\n\nENTRY CONTEXT");
            PrinterIAL.Print(entry.Program.Code);
            Console.WriteLine();

            foreach (var context in entry.ChildContexts)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Method: "+context.Name);
                PrinterIAL.Print(context.Program.Code);
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Elapsed time: {0}ms",watch.ElapsedMilliseconds);

            Console.ReadKey();

            
        }        
    }


}

