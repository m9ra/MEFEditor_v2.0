using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Reflection;

using TypeExperiments.Core;
using TypeExperiments.TypeBuilding;
using TypeExperiments.Reflection.ILAnalyzer;

using System.Diagnostics;

using UnitTesting.TypeSystem_TestUtils;


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
            var code = AssemblyUtils.Run(@"

var test=StaticClass.StaticMethod(""aaa"");
var test2=test;
var test3=4;
").AddMethod("StaticClass.StaticMethod", @"
        return ""ValueFromStaticCall"";
", true)
 .AddMethod("StaticClass.StaticClass", @"
    return ""Initialization value"";
", true)

 .GetResult().EntryContext.ProgramCode;

            PrinterIAL.Print(code);
        }



        


        
    }


}

