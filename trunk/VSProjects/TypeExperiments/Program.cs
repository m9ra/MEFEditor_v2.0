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
            var entry = AssemblyUtils.Run(/*@"
var test=StaticClass.StaticMethod(""aaa"",153);
var test2=test;
var test3=4;

if(true){
    test3=2;
}else{
    test3=1;
}
")
 
 .AddMethod("StaticClass.StaticMethod", @"
        return arg1;
", true, 
 new ParameterInfo("arg1",new TypeSystem.InstanceInfo("System.String")),
 new ParameterInfo("arg2",new TypeSystem.InstanceInfo("System.Int32"))
 )
 
 
 .AddMethod("StaticClass.StaticClass", @"
    return ""Initialization value"";
", true)
 */

@"
var result=fib(18);

").AddMethod("fib", @"    
    if(n<2){
        return 1;
    }else{
        return fib(n-1)+fib(n-2);
    }
", arguments: new ParameterInfo("n", new InstanceInfo("System.Int32")))
  

 .GetResult().EntryContext;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ENTRY CONTEXT");
            PrinterIAL.Print(entry.Program.Code);
            Console.WriteLine();

            foreach (var context in entry.ChildContexts)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Method: "+context.Name);
                PrinterIAL.Print(context.Program.Code);
                Console.WriteLine();
            }

            Console.ReadKey();

            
        }        
    }


}

