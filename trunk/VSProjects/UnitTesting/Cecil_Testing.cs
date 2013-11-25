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

        private TestingAssembly run(string methodName)
        {
            var assemblyFile = GetType().Assembly.Location;
            return AssemblyUtils.RunCECIL(assemblyFile, typeof(Cecil_Testing).FullName + "." + methodName);
        }
    }
}
