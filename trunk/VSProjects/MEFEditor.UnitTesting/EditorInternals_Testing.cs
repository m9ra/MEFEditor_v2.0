using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Analyzing.Execution.Instructions;
using UnitTesting.Analyzing_TestUtils;
using UnitTesting.TypeSystem_TestUtils;

using Analyzing;
using Analyzing.Execution;
using Analyzing.Editing;

using TypeSystem;
using AssemblyProviders.CSharp;
using AssemblyProviders.CSharp.Compiling;

using UnitTesting.AssemblyProviders_TestUtils;

namespace UnitTesting
{
    /// <summary>
    /// Testing of important editor classes
    /// </summary>
    [TestClass]
    public class EditorInternals_Testing
    {
        [TestMethod]
        public void TypeDescriptor_SimpleType()
        {
            typeof(string)
            .AssertFullname("System.String");
        }

        [TestMethod]
        public void TypeDescriptor_GenericType()
        {
            typeof(List<string>)
            .AssertFullname("System.Collections.Generic.List<System.String>");
        }

        [TestMethod]
        public void TypeDescriptor_GenericDefinition()
        {
            typeof(Dictionary<,>)
            .AssertFullname("System.Collections.Generic.Dictionary<@0,@1>");
        }

        [TestMethod]
        public void TypeDescriptor_IndependentParameters()
        {
            TestClass.IndependentParamsInfo.ReturnType
            .AssertFullname("System.Collections.Generic.Dictionary<@0,@1>");
        }

        [TestMethod]
        public void TypeDescriptor_DependentParameters()
        {
            TestClass.DependentParamsInfo.ReturnType
            .AssertFullname("System.Collections.Generic.Dictionary<@0,@0>");
        }

        [TestMethod]
        public void TypeDescriptor_GenericNamespaceDefinition()
        {
            typeof(NamespaceClass<>.InnerClass<>)
            .AssertFullname("NamespaceClass<@0>.InnerClass<@1>");
        }

        [TestMethod]
        public void TypeDescriptor_GenericNamespaceType()
        {
            typeof(NamespaceClass<string>.InnerClass<int>)
            .AssertFullname("NamespaceClass<System.String>.InnerClass<System.Int32>");
        }

        [TestMethod]
        public void TypeDescriptor_LongGenericNamespaceType()
        {
            typeof(NamespaceClass<string>.NamespaceClass2<object>.InnerClass<int>)
            .AssertFullname("NamespaceClass<System.String>.NamespaceClass2<System.Object>.InnerClass<System.Int32>");
        }

        [TestMethod]
        public void TypeDescriptor_ArrayType()
        {
            typeof(NamespaceClass<string>.InnerClass<int>[])
            .AssertFullname("Array<NamespaceClass<System.String>.InnerClass<System.Int32>,1>");
        }

        [TestMethod]
        public void TypeDescriptor_NestedGenericType()
        {
            typeof(List<NamespaceClass<string>.InnerClass<int>>)
            .AssertFullname("System.Collections.Generic.List<NamespaceClass<System.String>.InnerClass<System.Int32>>");
        }

        [TestMethod]
        public void PathInfo_NoArg()
        {
            "Test.Call"
            .AssertPath("Test.Call");
        }

        [TestMethod]
        public void PathInfo_SingleArg()
        {
            "Test.Call<System.String>"
            .AssertPath("Test.Call<>", "System.String");
        }

        [TestMethod]
        public void PathInfo_DoubleArg()
        {
            "Test.Call<System.String,System.Int32>"
            .AssertPath("Test.Call<,>", "System.String", "System.Int32");
        }

        [TestMethod]
        public void PathInfo_NestedArg()
        {
            "Test.Call<List<System.String>>"
            .AssertPath("Test.Call<>", "List<System.String>");
        }

        [TestMethod]
        public void PathInfo_NonGenericPath_NoChange()
        {
            "Test.Call"
            .AssertNonGenericPath("Test.Call");
        }

        [TestMethod]
        public void PathInfo_NonGenericPath_Ending()
        {
            "Test.Call<List<System.String>>"
            .AssertNonGenericPath("Test.Call");
        }

        [TestMethod]
        public void PathInfo_NonGenericPath_Inside()
        {
            "Test<Generic>.Test2<Generic<Inside>>.Call<List<System.String>>"
            .AssertNonGenericPath("Test.Test2.Call");
        }

        [TestMethod]
        public void InstanceInfo_GenericSingleArgument()
        {
            Tools.AssertName<System.Collections.Generic.List<string>>("System.Collections.Generic.List<System.String>");
        }

        [TestMethod]
        public void InstanceInfo_GenericTwoArguments()
        {
            Tools.AssertName<Dictionary<string, int>>("System.Collections.Generic.Dictionary<System.String,System.Int32>");
        }

        [TestMethod]
        public void InstanceInfo_GenericNestedArgument()
        {
            Tools.AssertName<Dictionary<List<string>, int>>("System.Collections.Generic.Dictionary<System.Collections.Generic.List<System.String>,System.Int32>");
        }


        [TestMethod]
        public void InstanceInfo_GenericChainedArgument()
        {
            Tools.AssertName<TestClass<string>.NestedClass<int>>("UnitTesting.TestClass<System.String>.UnitTesting.NestedClass<System.Int32>");
        }
    }
}
