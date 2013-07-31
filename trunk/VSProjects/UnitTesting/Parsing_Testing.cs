using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Analyzing.Execution.Instructions;
using UnitTesting.Analyzing_TestUtils;
using UnitTesting.TypeSystem_TestUtils;

using Analyzing;
using TypeSystem;
using AssemblyProviders.CSharp;

namespace UnitTesting
{
    [TestClass]
    public class Parsing_Testing
    {
        [TestMethod]
        public void BasicParsing()
        {
            var parser = new SyntaxParser();
            var result = parser.Parse(@"{
var test=System.String.test;
var test2=System.String.test();
}
");
        }


        [TestMethod]
        public void Emit_variableAssign()
        {
            AssemblyUtils.Run(@"
var test=""hello"";
var test2=test;
").AssertVariable("test2").HasValue("hello");
        }

        [TestMethod]
        public void Emit_call()
        {
            AssemblyUtils.Run(@"
var test=ParsedMethod();

").AddMethod("ParsedMethod", @"
        return ""ParsedValue"";
").AssertVariable("test").HasValue("ParsedValue");
        }

        [TestMethod]
        public void Emit_staticCall()
        {
            AssemblyUtils.Run(@"
var test=StaticClass.StaticMethod();

").AddMethod("StaticClass.StaticMethod", @"
        return ""ValueFromStaticCall"";
", true).AssertVariable("test").HasValue("ValueFromStaticCall");
        }


    }
}
