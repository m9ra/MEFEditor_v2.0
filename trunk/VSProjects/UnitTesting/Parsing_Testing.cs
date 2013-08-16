using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Analyzing.Execution.Instructions;
using UnitTesting.Analyzing_TestUtils;
using UnitTesting.TypeSystem_TestUtils;

using Analyzing;
using TypeSystem;
using AssemblyProviders.CSharp;
using AssemblyProviders.CSharp.Compiling;


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
")
 .AssertVariable("test").HasValue("ParsedValue");
        }

        [TestMethod]
        public void Emit_staticCall()
        {
            AssemblyUtils.Run(@"
var test=StaticClass.StaticMethod();

").AddMethod("StaticClass.StaticMethod", @"
        return ""ValueFromStaticCall"";
", true)
 .AddMethod("StaticClass.StaticClass", @"
    return ""Initialization value"";
",true)
.AssertVariable("test").HasValue("ValueFromStaticCall");
        }

        [TestMethod]
        public void Emit_objectCall()
        {
            AssemblyUtils.Run(@"
var obj=""Test string"";
var result=obj.CustomMethod();
").AddMethod("System.String.CustomMethod", @"
    return ""Custom result"";
")
.AssertVariable("result").HasValue("Custom result");
        }


        [TestMethod]
        public void Emit_objectCall_withArguments()
        {
            AssemblyUtils.Run(@"
var obj=""Object value"";
var argument=""Argument value"";
var result=obj.CustomMethod(argument);
").AddMethod("System.String.CustomMethod", @"
    return parameterName;
",arguments: new ParameterInfo("parameterName",new InstanceInfo("System.String")))
.AssertVariable("result").HasValue("Argument value");
        }


    }
}
