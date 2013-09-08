using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Analyzing;
using TypeSystem;

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
    }
}
