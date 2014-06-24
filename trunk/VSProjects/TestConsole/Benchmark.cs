using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using TypeExperiments.Core;

namespace TypeExperiments
{
    static class Benchmark
    {
        public static void WrappTypeTest()
        {
            Console.WriteLine();
            var assembly = new InternalAssembly();
            var stringType = new TypeBuilding.TypeWrapping.WrappedType<string>(assembly);
            var testType = new TypeBuilding.TypeWrapping.WrappedType<TestToWrapp>(assembly);
            var intType = new TypeBuilding.TypeWrapping.WrappedType<int>(assembly);
            var testString = stringType.ConstructInstance("Testing string");
            var testInt = intType.ConstructInstance(4);
            var instance = testType.ConstructInstance();

            int constructCount=1000;
            var w=Stopwatch.StartNew();
            for (int i = 0; i < constructCount; ++i)
            {
                var testInst = testType.ConstructInstance();
            }
            w.Stop();
            Console.WriteLine("construct aprox time: {0:}ms", w.Elapsed.TotalSeconds/constructCount);


            int callCount = 1000000;
            w.Start();
            for (int i = 0; i < callCount; ++i)
            {
                var result = testType.Invoke(instance, "methodCall", testString, testInt);
            }
            w.Stop();
            Console.WriteLine("call aprox time: {0}ms", w.Elapsed.TotalSeconds/callCount);
        }
    }

    class TestToWrapp
    {
        TestToWrapp methodCall(string testString, int testInt)
        {
            //   Console.WriteLine(testString+testInt);
            return this;
        }
    }
}
