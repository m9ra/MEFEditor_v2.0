using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

using MEFEditor.Analyzing.Execution.Instructions;
using MEFEditor.UnitTesting.Analyzing_TestUtils;
using MEFEditor.UnitTesting.TypeSystem_TestUtils;

using MEFEditor.Analyzing;
using MEFEditor.Analyzing.Execution;
using MEFEditor.Analyzing.Editing;

using MEFEditor.TypeSystem;
using RecommendedExtensions.Core.Languages.CSharp;
using RecommendedExtensions.Core.Languages.CSharp.Compiling;

using RecommendedExtensions.Core.AssemblyProviders.CILAssembly;

namespace MEFEditor.UnitTesting
{
    /// <summary>
    /// Testing of CIL Compiler from Recommended Extensions.
    /// </summary>
    [TestClass]
    public class Compiler_CIL_Testing
    {
        #region Testing transcription from assemblies loaded by Mono.CECIL

        #region Testing transcription of for loop

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void Emit_CECILForLoop()
        {
            run("ForLoop")
                //                        0123456789
                .AssertReturn().HasValue("aaaaaaaaaa")
                ;
        }

        /// <summary>
        /// CECIL Method that is translated.
        /// </summary>
        /// <returns>Test result.</returns>
        static string ForLoop()
        {
            string str = "";
            for (int i = 0; i < 10; ++i)
            {
                str += "a";
            }

            return str;
        }

        #endregion

        #region Testing transcription of call outside assembly

        /// <summary>
        /// Test case.
        /// </summary>
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

        /// <summary>
        /// CECIL Method that is translated.
        /// </summary>
        /// <returns>Test result.</returns>
        static string InsideAssembly()
        {
            return "Inside";
        }

        #endregion

        #region Testing transcription of static initializers

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void Provide_CECILStaticInitializer()
        {
            runWithAssemblyLoad(() =>
            {
                return "Data" + _data;
            }).AssertReturn().HasValue("DataInitialized");
        }

        /// <summary>
        /// Test storage.
        /// </summary>
        private static string _data;

        /// <summary>
        /// CECIL Method that is translated.
        /// </summary>
        static Compiler_CIL_Testing()
        {
            _data = "Initialized";
        }

        #endregion

        #endregion

        #region Testing of transcription from instructions loaded by System.Reflection

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void Emit_CILSimple()
        {
            AssemblyUtils.RunCIL(() =>
            {
                var x = "hello";
                return x;
            })

            .AssertReturn().HasValue("hello")
            ;
        }

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void Emit_CILStaticCall()
        {
            AssemblyUtils.RunCIL(() =>
            {
                var x1 = "A";
                var x2 = "B";
                return string.Concat(x1, x2);
            })

            .AssertReturn().HasValue("AB")
            ;
        }

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void Emit_CILObjectCall()
        {
            AssemblyUtils.RunCIL(() =>
            {
                var x1 = "ABCD";
                return x1.Substring(2);
            })

            .AssertReturn().HasValue("CD")
            ;
        }

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void Emit_CILForLoop()
        {
            AssemblyUtils.RunCIL(() =>
            {
                string str = "";
                for (int i = 0; i < 10; ++i)
                {
                    str += "a";
                }

                return str;
            })
                //                    0123456789  
            .AssertReturn().HasValue("aaaaaaaaaa")
            ;
        }
        #endregion

        #region Testing utilities

        /// <summary>
        /// Runs the method with specified name.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <returns>TestingAssembly.</returns>
        private TestingAssembly run(string methodName)
        {
            var assemblyFile = GetType().Assembly.Location;
            return AssemblyUtils.RunCECIL(assemblyFile, typeof(Compiler_CIL_Testing).FullName + "." + methodName);
        }

        /// <summary>
        /// Runs specified method.
        /// </summary>
        /// <param name="entryMethod">The entry method.</param>
        /// <returns>TestingAssembly.</returns>
        private TestingAssembly runWithAssemblyLoad(Func<string> entryMethod)
        {
            var assembly = new CILAssembly(GetType().Assembly.Location);
            return AssemblyUtils.RunCIL(entryMethod)
                .AddAssembly(assembly);
        }

        #endregion
    }
}
