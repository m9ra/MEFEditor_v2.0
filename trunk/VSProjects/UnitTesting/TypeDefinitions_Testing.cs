using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Testing of <see cref="RuntimeTypeDefinition"/> handling by <see cref="RuntimeAssembly"/>.
    /// </summary>
    [TestClass]
    public class TypeDefinitions_Testing
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
        public void RuntimeType_DirectClassType()
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

        [TestMethod]
        public void RuntimeType_DirectGenericClassType()
        {
            AssemblyUtils.Run(@"                
                   var list=new System.Collections.Generic.List<System.String>();     
                   list.Add(""AddedValue"");

                   var result=list[0];
           ")

            .AddDirectToRuntime<System.Collections.Generic.List<string>>()

            .AssertVariable("result").HasValue("AddedValue")

            ;
        }

        [TestMethod]
        public void RuntimeType_DirectWrappedGenericClassType()
        {
            AssemblyUtils.Run(@"                
                   var list=new System.Collections.Generic.Dictionary<System.String,System.Int32>();     
                   list.Add(""key"", 1234);

                   var result=list[""key""];
               ")

            .AddWrappedGenericToRuntime(typeof(Dictionary<,>))

            .AssertVariable("result").HasValue(1234);

        }

        [TestMethod]
        public void RuntimeType_DirectWrappedGenericMethod()
        {
            AssemblyUtils.Run(@"                
                   var cls = new GenericClass<System.Int32>();     
                   var result = cls.GenericMethod(""Result"");
               ")

            .AddWrappedGenericToRuntime(typeof(GenericClass<>))

            .AssertVariable("result").HasValue("Result");
        }

        [TestMethod]
        public void RuntimeType_ArrayReturnValueSupport()
        {
            AssemblyUtils.Run(@"                
                   var list=new System.Collections.Generic.List<System.String>();
                   list.Add(""Item0"");                   
                   var arr=list.ToArray();
                   
                   var result=arr[0];
            ")

            .AddWrappedGenericToRuntime(typeof(List<>))

            .AssertVariable("result").HasValue("Item0");
            ;
        }
    }
}
