using System;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Analyzing;
using TypeSystem;
using TypeSystem.Runtime;

using UnitTesting.TypeSystem_TestUtils;
using UnitTesting.AssemblyProviders_TestUtils;

using UnitTesting.RuntimeTypeDefinitions;

namespace UnitTesting
{
    [TestClass]
    public class RuntimeAssembly_Testing
    {
        [TestMethod]
        public void RuntimeType_Call()
        {
            AssemblyUtils.Run(@"                
                var test=new SimpleType(""CtorValue"");      
                var result=test.Concat(""CallArg"");      
            ")

            .AddToRuntime<SimpleType>()
            
            .AssertVariable("result").HasValue("CtorValue_CallArg")

            ;
        }

        [TestMethod]
        public void RuntimeType_CallWithDefault()
        {
            AssemblyUtils.Run(@"                
                var test=new SimpleType(""CtorValue"");      
                var result=test.Concat();      
            ")

            .AddToRuntime<SimpleType>()

            .AssertVariable("result").HasValue("CtorValue_CallDefault")

            ;
        }

        [TestMethod]
        public void RuntimeType_DirectTypeWithoutInitializer()
        {
            AssemblyUtils.Run(@"                
                var test=new System.Text.StringBuilder();
                test.Append(""Data"");
                test.Append(""2"");
                var result=test.ToString();      
            ")

            .AddDirectToRuntime<StringBuilder>()

            .AssertVariable("result").HasValue("Data2")

            ;
        }

    }
}
