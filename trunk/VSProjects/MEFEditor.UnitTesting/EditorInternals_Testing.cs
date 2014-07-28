using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MEFEditor.Analyzing.Execution.Instructions;
using MEFEditor.UnitTesting.Analyzing_TestUtils;
using MEFEditor.UnitTesting.TypeSystem_TestUtils;

using MEFEditor.Analyzing;
using MEFEditor.Analyzing.Execution;
using MEFEditor.Analyzing.Editing;

using MEFEditor.TypeSystem;
using RecommendedExtensions.Core.Languages.CSharp;
using RecommendedExtensions.Core.Languages.CSharp.Compiling;

using MEFEditor.UnitTesting.AssemblyProviders_TestUtils;

namespace MEFEditor.UnitTesting
{
    /// <summary>
    /// Testing of important editor classes
    /// </summary>
    [TestClass]
    public class EditorInternals_Testing
    {

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void TypeDescriptor_SimpleType()
        {
            typeof(string)
            .AssertFullname("System.String");
        }

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void TypeDescriptor_GenericType()
        {
            typeof(List<string>)
            .AssertFullname("System.Collections.Generic.List<System.String>");
        }

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void TypeDescriptor_GenericDefinition()
        {
            typeof(Dictionary<,>)
            .AssertFullname("System.Collections.Generic.Dictionary<@0,@1>");
        }

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void TypeDescriptor_IndependentParameters()
        {
            TestClass.IndependentParamsInfo.ReturnType
            .AssertFullname("System.Collections.Generic.Dictionary<@0,@1>");
        }

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void TypeDescriptor_DependentParameters()
        {
            TestClass.DependentParamsInfo.ReturnType
            .AssertFullname("System.Collections.Generic.Dictionary<@0,@0>");
        }

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void TypeDescriptor_GenericNamespaceDefinition()
        {
            typeof(NamespaceClass<>.InnerClass<>)
            .AssertFullname("NamespaceClass<@0>.InnerClass<@1>");
        }

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void TypeDescriptor_GenericNamespaceType()
        {
            typeof(NamespaceClass<string>.InnerClass<int>)
            .AssertFullname("NamespaceClass<System.String>.InnerClass<System.Int32>");
        }

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void TypeDescriptor_LongGenericNamespaceType()
        {
            typeof(NamespaceClass<string>.NamespaceClass2<object>.InnerClass<int>)
            .AssertFullname("NamespaceClass<System.String>.NamespaceClass2<System.Object>.InnerClass<System.Int32>");
        }

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void TypeDescriptor_ArrayType()
        {
            typeof(NamespaceClass<string>.InnerClass<int>[])
            .AssertFullname("Array<NamespaceClass<System.String>.InnerClass<System.Int32>,1>");
        }

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void TypeDescriptor_NestedGenericType()
        {
            typeof(List<NamespaceClass<string>.InnerClass<int>>)
            .AssertFullname("System.Collections.Generic.List<NamespaceClass<System.String>.InnerClass<System.Int32>>");
        }

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void PathInfo_NoArg()
        {
            "Test.Call"
            .AssertPath("Test.Call");
        }

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void PathInfo_SingleArg()
        {
            "Test.Call<System.String>"
            .AssertPath("Test.Call<>", "System.String");
        }

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void PathInfo_DoubleArg()
        {
            "Test.Call<System.String,System.Int32>"
            .AssertPath("Test.Call<,>", "System.String", "System.Int32");
        }

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void PathInfo_NestedArg()
        {
            "Test.Call<List<System.String>>"
            .AssertPath("Test.Call<>", "List<System.String>");
        }

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void PathInfo_NonGenericPath_NoChange()
        {
            "Test.Call"
            .AssertNonGenericPath("Test.Call");
        }

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void PathInfo_NonGenericPath_Ending()
        {
            "Test.Call<List<System.String>>"
            .AssertNonGenericPath("Test.Call");
        }

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void PathInfo_NonGenericPath_Inside()
        {
            "Test<Generic>.Test2<Generic<Inside>>.Call<List<System.String>>"
            .AssertNonGenericPath("Test.Test2.Call");
        }

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void InstanceInfo_GenericSingleArgument()
        {
            Tools.AssertName<System.Collections.Generic.List<string>>("System.Collections.Generic.List<System.String>");
        }

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void InstanceInfo_GenericTwoArguments()
        {
            Tools.AssertName<Dictionary<string, int>>("System.Collections.Generic.Dictionary<System.String,System.Int32>");
        }

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void InstanceInfo_GenericNestedArgument()
        {
            Tools.AssertName<Dictionary<List<string>, int>>("System.Collections.Generic.Dictionary<System.Collections.Generic.List<System.String>,System.Int32>");
        }

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void InstanceInfo_GenericChainedArgument()
        {
            Tools.AssertName<TestClass<string>.NestedClass<int>>("MEFEditor.UnitTesting.TestClass<System.String>.MEFEditor.UnitTesting.NestedClass<System.Int32>");
        }
    }
}
