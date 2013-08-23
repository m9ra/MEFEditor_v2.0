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
            var result = parser.Parse(new Source(@"{
var test=System.String.test;
var test2=System.String.test();
}
"));
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
",parameters: new ParameterInfo("parameterName",new InstanceInfo("System.String")))
.AssertVariable("result").HasValue("Argument value");
        }

        [TestMethod]
        public void Emit_Fibonacci()
        {
            //fib(24) Time elapsed: 16s (without caching)
            //fib(24) Time elapsed: 15s (IInstructionLoader, IInstructionGenerator to abstract classes)
            //fib(24) Time elapsed:  1s (with caching)
            //fib(29) Time elapsed: 14s (with caching)
            AssemblyUtils.Run(@"
var result=fib(7);

").AddMethod("fib", @"    
    if(n<3){
        return 1;
    }else{
        return fib(n-1)+fib(n-2);
    }
", parameters: new ParameterInfo("n", new InstanceInfo("System.Int32")))


.AssertVariable("result").HasValue(13);
        }


        [TestMethod]
        public void Edit_SimpleReject()
        {
            AssemblyUtils.Run(@"
var arg=""input"";
DirectMethod(arg);

").AddMethod("DirectMethod", (c) =>
 {
     var arg = c.CurrentArguments[1];
     c.Edits.RemoveArgument(arg, 1, ".reject");
 }, false, new ParameterInfo("parameter", new InstanceInfo("System.String")))

.AddEditAction("arg", ".reject").AssertSourceEquivalence(@"

var arg=""input"";
DirectMethod();
");

        }

    }
}
