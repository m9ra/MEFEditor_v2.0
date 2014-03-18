using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

using Analyzing.Execution.Instructions;
using UnitTesting.Analyzing_TestUtils;
using UnitTesting.TypeSystem_TestUtils;

using Analyzing;
using Analyzing.Execution;
using Analyzing.Editing;

using TypeSystem;
using AssemblyProviders.CSharp;
using AssemblyProviders.CSharp.Compiling;

using AssemblyProviders.CILAssembly;

namespace UnitTesting
{
    [TestClass]
    public class Cecil_Testing
    {
        [TestMethod]
        public void Emit_CECILForLoop()
        {
            run("ForLoop")
                //                        0123456789
                .AssertReturn().HasValue("aaaaaaaaaa")
                ;
        }

        static string ForLoop()
        {
            string str = "";
            for (int i = 0; i < 10; ++i)
            {
                str += "a";
            }

            return str;
        }

        [TestMethod]
        public void Provide_CECILCrossCall()
        {
            runWithAssemblyLoad(() =>
            {
                var x = "Outside";
                var y = InsideAssembly();
                return x + "_" + y;
            }).AssertReturn().HasValue("Outside_Inside");
        }

        static string InsideAssembly()
        {
            return "Inside";
        }

        [TestMethod]
        public void Provide_CECILStaticInitializer()
        {
            runWithAssemblyLoad(() =>
            {
                return "Data" + _data;
            }).AssertReturn().HasValue("DataInitialized");
        }

        private static string _data;

        static Cecil_Testing()
        {
            _data = "Initialized";
        }

        #region Testing utilities

        private TestingAssembly run(string methodName)
        {
            var assemblyFile = GetType().Assembly.Location;
            return AssemblyUtils.RunCECIL(assemblyFile, typeof(Cecil_Testing).FullName + "." + methodName);
        }

        private TestingAssembly runWithAssemblyLoad(Func<string> entryMethod)
        {
            var assembly = new CILAssembly(GetType().Assembly.Location);
            return AssemblyUtils.RunCIL(entryMethod)
                .AddAssembly(assembly);
        }

        #endregion
    }
}
