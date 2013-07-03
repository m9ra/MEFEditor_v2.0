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


namespace TypeExperiments
{
    class Program
    {

        string x;
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


    }

   
}

