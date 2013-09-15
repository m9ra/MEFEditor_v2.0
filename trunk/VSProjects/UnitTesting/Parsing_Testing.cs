using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Analyzing.Execution.Instructions;
using UnitTesting.Analyzing_TestUtils;
using UnitTesting.TypeSystem_TestUtils;

using Analyzing;
using Analyzing.Execution;
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
            }"));
        }


        [TestMethod]
        public void Emit_variableAssign()
        {
            AssemblyUtils.Run(@"
                var test=""hello"";
                var test2=test;
            ")

            .AssertVariable("test2").HasValue("hello");
        }

        [TestMethod]
        public void Emit_call()
        {
            AssemblyUtils.Run(@"
                var test=ParsedMethod();
            ")

            .AddMethod("Test.ParsedMethod", @"
                return ""ParsedValue"";
            ", Method.String_NoParam)

            .AssertVariable("test").HasValue("ParsedValue");
        }

        [TestMethod]
        public void Emit_staticCall()
        {
            AssemblyUtils.Run(@"
                var test=StaticClass.StaticMethod(""CallArg"");
            ")

            .AddMethod("StaticClass.StaticMethod", (c) =>
            {
                var self = c.CurrentArguments[0];
                var arg = c.CurrentArguments[1].DirectValue as string;
                var field = c.GetField(self, "StaticField");

                var result = c.CreateDirectInstance(field + "_" + arg);
                c.Return(result);
            }
            , Method.StaticString_StringParam)

            .AddMethod("StaticClass.#initializer", (c) =>
            {
                var self = c.CurrentArguments[0];
                c.SetField(self, "StaticField", "InitValue");
            }
            , Method.StaticInitializer)

            .AssertVariable("test").HasValue("InitValue_CallArg");
        }

        [TestMethod]
        public void Emit_objectCall()
        {
            AssemblyUtils.Run(@"
                var obj=""Test string"";
                var result=obj.CustomMethod();
            ")

            .AddMethod("System.String.CustomMethod", @"
                return ""Custom result"";
            ", Method.String_NoParam)

            .AssertVariable("result").HasValue("Custom result");
        }


        [TestMethod]
        public void Emit_objectCall_withArguments()
        {
            AssemblyUtils.Run(@"
                var obj=""Object value"";
                var argument=""Argument value"";
                var result=obj.CustomMethod(argument);
            ")

            .AddMethod("System.String.CustomMethod", @"
                return p;
             ", Method.String_StringParam)

            .AssertVariable("result").HasValue("Argument value");
        }

        [TestMethod]
        public void Emit_objectCreation_fieldsUsage()
        {
            AssemblyUtils.Run(@"
                var obj=new TestObj(""input"");
                
                var result = obj.GetInput();          
            ")

            .AddMethod("TestObj.TestObj", (c) =>
            {
                var arg = c.CurrentArguments[1];
                var thisObj = c.CurrentArguments[0];

                c.SetField(thisObj, "inputData", arg);

            }, Method.Ctor_StringParam)

            .AddMethod("TestObj.GetInput", (c) =>
            {
                var thisObj = c.CurrentArguments[0];
                var data = c.GetField(thisObj, "inputData") as Instance;
                c.Return(data);
            }, Method.String_NoParam)

            .AssertVariable("result").HasValue("input");
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
            ")

            .AddMethod("Test.fib", @"    
                if(n<3){
                    return 1;
                }else{
                    return fib(n-1)+fib(n-2);
                }
            ", Method.Int_IntParam)

            .AssertVariable("result").HasValue(13);
        }


        [TestMethod]
        public void Edit_SimpleReject()
        {
            AssemblyUtils.Run(@"
                var arg=""input"";
                DirectMethod(arg);
            ")

            .AddMethod("Test.DirectMethod", (c) =>
            {
                var arg = c.CurrentArguments[1];
                c.Edits.RemoveArgument(arg, 1, ".reject");
            }, Method.Void_StringParam)

            .AddEditAction("arg", ".reject")
            .AssertSourceEquivalence(@"
                var arg=""input"";
                DirectMethod();
            ");
        }

        [TestMethod]
        public void Edit_RejectWithSideEffect()
        {
            AssemblyUtils.Run(@"
                var arg=""input"";
                DirectMethod(arg=""input2"");
            ")

            .AddMethod("Test.DirectMethod", (c) =>
            {
                var arg = c.CurrentArguments[1];
                c.Edits.RemoveArgument(arg, 1, ".reject");
            }, Method.Void_StringParam)

            .AddEditAction("arg", ".reject")
            .AssertSourceEquivalence(@"
                var arg=""input"";
                arg=""input2"";
                DirectMethod();
            ");
        }


        [TestMethod]
        public void Edit_RewriteWithSideEffect()
        {
            AssemblyUtils.Run(@"
                var arg=""input"";
                DirectMethod(arg=""input2"");
            ")

            .AddMethod("Test.DirectMethod", (c) =>
            {
                var arg = c.CurrentArguments[1];
                c.Edits.ChangeArgument(arg, 1, "Change", (s) => "input3");
            }, Method.Void_StringParam)

            .AddEditAction("arg", "Change")
            .AssertSourceEquivalence(@"
                var arg=""input"";
                arg=""input2"";
                DirectMethod(""input3"");
            ");
        }

        [TestMethod]
        public void Edit_AppendWithValidScope()
        {
            AssemblyUtils.Run(@"
                var arg=""input"";
                DirectMethod(""input2"");
            ")

            .AddMethod("Test.DirectMethod", (c) =>
            {
                var thisInst = c.CurrentArguments[0];
                var e = c.Edits;
                c.Edits.AppendArgument(thisInst, "Append", (s) => e.GetVariableFor(AssemblyUtils.EXTERNAL_INPUT, s));
            }, Method.Void_StringParam)

            .UserAction((c) =>
            {
                AssemblyUtils.EXTERNAL_INPUT = c.EntryContext.GetValue(new VariableName("arg"));
            })

            .AddEditAction("this", "Append")

            .AssertSourceEquivalence(@"
                var arg=""input"";
                DirectMethod(""input2"",arg);
            ");
        }

        [TestMethod]
        public void Edit_AppendScopeEndShifting()
        {
            AssemblyUtils.Run(@"
                var arg=""input"";
                Report(arg);

                arg=""scope end"";
                arg=""tight scope end"";
                var arg2=""spliting line"";
                arg=""another scope end"";

                DirectMethod(""input2"");             
            ")

            .AddMethod("Test.DirectMethod", (c) =>
            {
                var thisInst = c.CurrentArguments[0];
                var e = c.Edits;
                e.AppendArgument(thisInst, "Append", (s) => e.GetVariableFor(AssemblyUtils.EXTERNAL_INPUT, s));

            }, Method.Void_StringParam)


            .UserAction((c) =>
            {
                AssemblyUtils.EXTERNAL_INPUT = AssemblyUtils.REPORTED_INSTANCE;
            })

            .AddEditAction("this", "Append")

            .AssertSourceEquivalence(@"
                var arg=""input"";
                Report(arg);

                var arg2=""spliting line"";
                DirectMethod(""input2"",arg);  

                arg=""scope end"";
                arg=""tight scope end"";
                arg=""another scope end"";
            ");
        }

        [TestMethod]
        public void Edit_AppendScopeStartShifting()
        {
            AssemblyUtils.Run(@"
                DirectMethod(""input2"");             
                var arg2=""spliting line"";

                var arg=""input"";
                Report(arg);
                              
                arg=""scope end"";
            ")

            .AddMethod("Test.DirectMethod", (c) =>
            {
                var thisInst = c.CurrentArguments[0];
                var e = c.Edits;
                e.AppendArgument(thisInst, "Append", (s) => e.GetVariableFor(AssemblyUtils.EXTERNAL_INPUT, s));

            }, Method.Void_StringParam)


            .UserAction((c) =>
            {
                AssemblyUtils.EXTERNAL_INPUT = AssemblyUtils.REPORTED_INSTANCE;
            })

            .AddEditAction("this", "Append")

            .AssertSourceEquivalence(@"
                var arg2=""spliting line"";

                var arg=""input"";
                DirectMethod(""input2"",arg);                  
                
                Report(arg);
                arg=""scope end"";
            ");
        }

        [TestMethod]
        public void Edit_RemoveWithRedeclaration()
        {
            AssemblyUtils.Run(@"
                var toDelete=""toDelete"";
                
                var anotherDeleted=PassThrough(toDelete);
                
                anotherDeleted=""force redeclaration"";                  
            ")

         .AddMethod("Test.PassThrough", (c) =>
         {
             var arg = c.CurrentArguments[1];
             c.Return(arg);
         }, Method.String_StringParam)

         .AddRemoveAction("toDelete")

         .AssertSourceEquivalence(@"
            var anotherDeleted=""force redeclaration"";         
         ");
        }

        [TestMethod]
        public void Edit_RemoveFromOptionalParam()
        {
            AssemblyUtils.Run(@"
                var toDelete=""toDelete"";                
                CallWithOptional(toDelete);                
            ")

         .AddMethod("Test.CallWithOptional", (c) =>
         {
             var arg = c.CurrentArguments[1];
             c.Edits.SetOptional(1);
             c.Return(arg);
         }, Method.String_StringParam)

         .AddRemoveAction("toDelete")

         .AssertSourceEquivalence(@"
            CallWithOptional();         
         ");
        }

        [TestMethod]
        public void Edit_RemoveFromCallCascade()
        {
            AssemblyUtils.Run(@"
            var toDelete=""toDelete"";                
            CallWithOptional(CallWithRequired(toDelete));                
         ")

            .AddMethod("Test.CallWithOptional", (c) =>
            {
                var arg = c.CurrentArguments[1];
                c.Edits.SetOptional(1);
                c.Return(arg);
            }, Method.String_StringParam)

            .AddMethod("Test.CallWithRequired", (c) =>
            {
                var arg = c.CurrentArguments[1];
                c.Return(arg);
            }, Method.String_StringParam)

            .AddRemoveAction("toDelete")

            .AssertSourceEquivalence(@"
            CallWithOptional();         
         ");
        }

        [TestMethod]
        public void Edit_RemoveMultiVariable()
        {
            AssemblyUtils.Run(@"                
                var toDelete=""toDelete"";
                var a=toDelete;
                var b=a;
            ")

         .AddRemoveAction("toDelete")

         .AssertSourceEquivalence(@"
                
         ");
        }


        [TestMethod]
        public void Edit_RemoveChainedAssign()
        {
            AssemblyUtils.Run(@"
                var a=""valA"";
                var b=""toDelete"";
                var c=""valC"";
        
                var toDelete=b;
                a=b=c;              
            ")

         .AddRemoveAction("toDelete")

         .AssertSourceEquivalence(@"
                var a=""valA"";                
                var c=""valC"";
                
                System.String b;        
                a=b=c;      
         ");
        }
    }
}
