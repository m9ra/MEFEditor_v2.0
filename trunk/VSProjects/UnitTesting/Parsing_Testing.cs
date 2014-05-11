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
    [TestClass]
    public class Parsing_Testing
    {
        [TestMethod]
        public void Lexer_SimpleGeneric()
        {
            "a < b . c >"
            .AssertTokens("a < b . c >");
        }

        [TestMethod]
        public void Lexer_GenericCall()
        {
            "a.b<c.d<e>>(f,g)"
            .AssertTokens("a", ".", "b<c.d<e>>", "(", "f", ",", "g", ")");
        }

        [TestMethod]
        public void Lexer_ComparisonLikeExpression()
        {
            "a < b c > d"
            .AssertTokens("a", "<", "b", "c", ">", "d");
        }

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

        [TestMethod]
        public void Parse_Basic()
        {
            var parser = new SyntaxParser();
            var result = parser.Parse(new Source(@"{
                var test=System.String.test;
                var test2=System.String.test();
            }", Method.EntryInfo));
        }

        [TestMethod]
        public void Compile_VariableAssign()
        {
            AssemblyUtils.Run(@"
                var test=""hello"";
                var test2=test;
            ")

            .AssertVariable("test2").HasValue("hello");
        }

        [TestMethod]
        public void Compile_Call()
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
        public void Compile_StaticCall()
        {
            AssemblyUtils.Run(@"
                var test=StaticClass.StaticMethod(""CallArg"");
            ")

            .AddMethod("StaticClass.StaticMethod", (c) =>
            {
                var self = c.CurrentArguments[0];
                var arg = c.CurrentArguments[1].DirectValue as string;
                var field = c.GetField(self, "StaticField");

                var result = c.Machine.CreateDirectInstance(field + "_" + arg);
                c.Return(result);
            }
            , Method.StaticString_StringParam)

            .AddMethod("StaticClass." + Naming.ClassCtorName, (c) =>
            {
                var self = c.CurrentArguments[0];
                c.SetField(self, "StaticField", "InitValue");
            }
            , Method.StaticInitializer)

            .AssertVariable("test").HasValue("InitValue_CallArg");
        }

        [TestMethod]
        public void Compile_ObjectCall()
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
        public void Compile_ObjectCall_WithArguments()
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
        public void Compile_ObjectCreation_FieldsUsage()
        {
            AssemblyUtils.Run(@"
                var obj=new TestObj(""input"");
                
                var result = obj.GetInput();          
            ")

            .AddMethod("TestObj.#ctor", (c) =>
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
        public void Compile_ObjectCreation_Alone()
        {
            AssemblyUtils.Run(@"                
                new TestObj();  
                var result=""after"";              
            ")

            .AddMethod("TestObj.#ctor", (c) =>
            {
            }, Method.Ctor_NoParam)

            .AssertVariable("result").HasValue("after");
        }

        [TestMethod]
        public void Compile_ObjectCreation_Nested()
        {
            AssemblyUtils.Run(@"                
                var x=new TestObj();  
                x.Nesting(new TestObj());
                var result=""after"";              
            ")

            .AddMethod("TestObj.#ctor", (c) => { }, Method.Ctor_NoParam)

            .AddMethod("TestObj.Nesting", (c) => { }, Method.Void_ObjectParam)

            .AssertVariable("result").HasValue("after");
        }

        [TestMethod]
        public void Compile_ObjectCreation_Return()
        {
            AssemblyUtils.Run(@"                
                var x=new TestObj();  
                x.Returning();
                var result=""after"";              
            ")

            .AddMethod("TestObj.#ctor", (c) => { }, Method.Ctor_NoParam)

            .AddMethod("TestObj.Returning", @"
                return new TestObj();
            ", Method.Object_NoParam)

            .AssertVariable("result").HasValue("after");
        }

        [TestMethod]
        public void Compile_ObjectCreation_Setter()
        {
            AssemblyUtils.Run(@"                
                var x=new TestObj();  
                x.Property=new TestObj();
                var result=""after"";              
            ")

            .AddMethod("TestObj.#ctor", (c) => { }, Method.Ctor_NoParam)

            .AddMethod("TestObj.set_Property", (c) => { }, Method.Void_ObjectParam)

            .AssertVariable("result").HasValue("after");
        }


        [TestMethod]
        public void Compile_ImplicitThis_Setter()
        {
            AssemblyUtils.Run(@"
                var obj=new TestObj();
                var result=obj.Property;
            ")

            .AddMethod("TestObj.#ctor", @"
                Property=""ValueToSet"";                
            ", Method.Ctor_NoParam)

            .AddMethod("TestObj.set_Property", (c) =>
            {
                var arg = c.CurrentArguments[1];
                var thisObj = c.CurrentArguments[0];

                c.SetField(thisObj, "_property", arg);

            }, Method.Ctor_StringParam)

            .AddMethod("TestObj.get_Property", (c) =>
            {
                var thisObj = c.CurrentArguments[0];
                var data = c.GetField(thisObj, "_property") as Instance;
                c.Return(data);
            }, Method.String_NoParam)

            .AssertVariable("result").HasValue("ValueToSet");
        }

        [TestMethod]
        public void Compile_Fibonacci()
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
        public void Compile_SwitchBlock()
        {
            AssemblyUtils.Run(@"                
                var result=5;
                switch(result){
                    case 1:
                        result=1;
                        break;  
                    case 2:
                        result=1;
                        break;
                    case 5:
                        result=55;
                        break;
                    default:
                        result=1;
                        break;
                }                
            ")

             .AssertVariable("result").HasValue(55);
        }

        [TestMethod]
        public void Compile_SwitchBlockContinue()
        {
            AssemblyUtils.Run(@"                
                var result="""";

                for(var i=0;i<3;++i){                
                    switch(i){
                        case 0:
                            result=result+""a"";
                            break;  
                        case 1:
                            result=result+""b"";
                            continue;                          
                        default:
                            result=result+""c"";
                            break;
                    }      

                    result=result+"";"";
                }          
            ")

             .AssertVariable("result").HasValue("a;bc;");
        }

        [TestMethod]
        public void Compile_ForLoop()
        {
            AssemblyUtils.Run(@"                
                var result=0;
                for(var i=0;i<10;++i){
                    result=result+2;                    
                }
                ++result;
            ")

             .AssertVariable("result").HasValue(21);
        }

        [TestMethod]
        public void Compile_ForLoopBreak()
        {
            AssemblyUtils.Run(@"                
                var result=0;
                for(var i=0;i<10;++i){
                    result=result+2;            
                    break;        
                }
                ++result;
            ")

             .AssertVariable("result").HasValue(3);
        }

        [TestMethod]
        public void Compile_ForLoopContinue()
        {
            AssemblyUtils.Run(@"                
                var result=0;
                var incr;
                for(incr=0;incr<10;++incr){
                    continue;
                    result=result+2;                                
                }
                ++result;
            ")

            .AssertVariable("result").HasValue(1)
            .AssertVariable("incr").HasValue(10);
        }

        [TestMethod]
        public void Compile_WhileLoop()
        {
            AssemblyUtils.Run(@"
                var i=0;
                var result=0;
                while(i<10){
                    result=result+2;
                    ++i;
                }
                ++result;
            ")

             .AssertVariable("result").HasValue(21);
        }

        [TestMethod]
        public void Compile_GenericCall()
        {
            AssemblyUtils.Run(@"                
                var test=new Test();     
                var result=test.Generic<Test2>(""GenericCallArg"");
            ")

            .AddMethod("Test." + Naming.CtorName, @"
                
            ", Method.Ctor_NoParam)

            .AddMethod("Test.Generic<T1>", @"
                var x=new T1(p);
                return x.GetValue();
            ", Method.Void_StringParam)

            .AddMethod("Test2." + Naming.CtorName, (c) =>
            {
                var thisObj = c.CurrentArguments[0];
                var arg = c.CurrentArguments[1];
                c.SetField(thisObj, "value", arg.DirectValue);
            }, Method.Ctor_StringParam)

            .AddMethod("Test2.GetValue", (c) =>
            {
                var thisObj = c.CurrentArguments[0];
                var value = c.GetField(thisObj, "value") as string;
                var result = c.Machine.CreateDirectInstance("Test2_" + value, TypeDescriptor.Create<string>());
                c.Return(result);
            }, Method.String_NoParam)

            .AssertVariable("result").HasValue("Test2_GenericCallArg");
        }

        [TestMethod]
        public void Compile_VirtualCall()
        {
            AssemblyUtils.Run(@"
                var test=new Test();
                
                var interface=Convert(test);
                interface.Add(""AddedValue"");
                var result=test.Get();
            ")

            .AddMethod("Test.Convert", (c) =>
            {
                c.Return(c.CurrentArguments[1]);
            }, Method.StringICollection_StringICollectionParam)

            .AddMethod("Test." + Naming.CtorName, (c) => { }, Method.Ctor_NoParam)

            .AddMethod("Test.Add", (c) =>
            {
                var thisInstance = c.CurrentArguments[0];
                var arg = c.CurrentArguments[1];
                c.SetField(thisInstance, "data", arg);
            }, Method.Void_StringParam.Implements(typeof(ICollection<string>)))

            .AddMethod("Test.Get", (c) =>
            {
                var thisInstance = c.CurrentArguments[0];
                var inst = c.GetField(thisInstance, "data") as Instance;
                c.Return(inst);
            }, Method.String_NoParam)

            .AddWrappedGenericToRuntime(typeof(ICollection<>))

            .DefineInheritance("Test", typeof(ICollection<string>))

            .AssertVariable("result").HasValue("AddedValue");



            ;
        }

        [TestMethod]
        public void Compile_VirtualGenericCall()
        {
            AssemblyUtils.Run(@"
                var list=new System.Collections.Generic.List<string>();               
                var interface=Convert(list);
                interface.Add(""AddedValue"");
                var result=list[0];
            ")

            .AddMethod("Test.Convert", (c) =>
            {
                c.Return(c.CurrentArguments[1]);
            }, Method.StringICollection_StringICollectionParam)

            .AddWrappedGenericToRuntime(typeof(List<>))
            .AddWrappedGenericToRuntime(typeof(ICollection<>))

            .AssertVariable("result").HasValue("AddedValue")
            ;
        }

        [TestMethod]
        public void Compile_ParamCall()
        {
            AssemblyUtils.Run(@"
                var formated=System.String.Format(""{0}{1}{2}"",""a"",""b"",""c"");               
            ")

            .AssertVariable("formated").HasValue(string.Format("{0}{1}{2}", "a", "b", "c"))
            ;
        }

        [TestMethod]
        public void Compile_Operators()
        {
            AssemblyUtils.Run(@"
                var inc=1;
                ++inc;                
                var post=inc++;
                var pref=++inc;            
                inc++;
            ")

            .AssertVariable("inc").HasValue(5)
            .AssertVariable("post").HasValue(2)
            .AssertVariable("pref").HasValue(4)
            ;
        }


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
                //                        0123456789  
            .AssertReturn().HasValue("aaaaaaaaaa")
            ;
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
                c.Edits.SetOptional(1);
            }, Method.Void_StringParam)

            .RunEditAction("arg", ".reject")
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
                c.Edits.SetOptional(1);
            }, Method.Void_StringParam)

            .RunEditAction("arg", ".reject")
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

            .RunEditAction("arg", "Change")
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
                c.Edits.AppendArgument(thisInst, 2, "Append", (s) => e.GetVariableFor(AssemblyUtils.EXTERNAL_INPUT, s));
            }, Method.Void_StringParam)

            .UserAction((c) =>
            {
                AssemblyUtils.EXTERNAL_INPUT = c.EntryContext.GetValue(new VariableName("arg"));
            })

            .RunEditAction("this", "Append")

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
                e.AppendArgument(thisInst, 2, "Append", (s) => e.GetVariableFor(AssemblyUtils.EXTERNAL_INPUT, s));

            }, Method.Void_StringParam)


            .UserAction((c) =>
            {
                AssemblyUtils.EXTERNAL_INPUT = AssemblyUtils.REPORTED_INSTANCE;
            })

            .RunEditAction("this", "Append")

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
                e.AppendArgument(thisInst, 2, "Append", (s) => e.GetVariableFor(AssemblyUtils.EXTERNAL_INPUT, s));

            }, Method.Void_StringParam)


            .UserAction((c) =>
            {
                AssemblyUtils.EXTERNAL_INPUT = AssemblyUtils.REPORTED_INSTANCE;
            })

            .RunEditAction("this", "Append")

            .AssertSourceEquivalence(@"
                var arg2=""spliting line"";

                var arg=""input"";
                DirectMethod(""input2"",arg);                  
                
                Report(arg);
                arg=""scope end"";
            ");
        }

        [TestMethod]
        public void Edit_AcceptWithValidScope()
        {
            AssemblyUtils.Run(@"
                var accepter=""accepter"";
                AddAccept(accepter);             

                var toAccept=""accepted"";
                Report(toAccept);
            ")

            .AddMethod("Test.AddAccept", (c) =>
            {
                var thisObj = c.CurrentArguments[0];
                var accepter = c.CurrentArguments[1];
                var e = c.Edits;

                e.AddCall(thisObj, "Accept", (view) =>
                {
                    return new CallEditInfo(accepter, "Accept", AssemblyUtils.EXTERNAL_INPUT);
                });

            }, Method.Void_StringParam)


            .UserAction((c) =>
            {
                AssemblyUtils.EXTERNAL_INPUT = AssemblyUtils.REPORTED_INSTANCE;
            })

            .RunEditAction("this", "Accept")

            .AssertSourceEquivalence(@"
                var accepter=""accepter"";
                AddAccept(accepter);             

                var toAccept=""accepted"";
                accepter.Accept(toAccept);
                Report(toAccept);
            ");
        }

        [TestMethod]
        public void Edit_AcceptWithEndScopeShifting()
        {
            AssemblyUtils.Run(@"
                var accepter=""accepter"";
                AddAccept(accepter);             

                accepter=""scope end"";
                var toAccept=""accepted"";

                Report(toAccept);
            ")

            .AddMethod("Test.AddAccept", (c) =>
            {
                var thisObj = c.CurrentArguments[0];
                var accepter = c.CurrentArguments[1];
                var e = c.Edits;

                e.AddCall(thisObj, "Accept", (view) =>
                {
                    return new CallEditInfo(accepter, "Accept", AssemblyUtils.EXTERNAL_INPUT);
                });

            }, Method.Void_StringParam)


            .UserAction((c) =>
            {
                AssemblyUtils.EXTERNAL_INPUT = AssemblyUtils.REPORTED_INSTANCE;
            })

            .RunEditAction("this", "Accept")

            .AssertSourceEquivalence(@"
                var accepter=""accepter"";
                AddAccept(accepter);             
                
                var toAccept=""accepted"";
                accepter.Accept(toAccept);
                accepter=""scope end"";
                Report(toAccept);
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

         .RunRemoveAction("toDelete")

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

         .RunRemoveAction("toDelete")

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

            .RunRemoveAction("toDelete")

            .AssertSourceEquivalence(@"
                CallWithOptional();         
            ");
        }

        [TestMethod]
        public void Edit_RemovePreserveCtor()
        {
            AssemblyUtils.Run(@"            
                var obj=new TestObj(""toDelete"");
                obj.CallWithRequired(new TestObj(""abc""));                
            ")

            .AddMethod("TestObj." + Naming.CtorName, (c) => { }, Method.Ctor_StringParam)

            .AddMethod("TestObj.CallWithRequired", (c) =>
            {
                var arg = c.CurrentArguments[1];
                c.Return(arg);
            }, Method.Void_ObjectParam)

            .RunRemoveAction("obj")

            .AssertSourceEquivalence(@"
                new TestObj(""abc"");         
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

            .RunRemoveAction("toDelete")

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

         .RunRemoveAction("toDelete")

         .AssertSourceEquivalence(@"
                var a=""valA"";                
                var c=""valC"";
                
                System.String b;        
                a=b=c;      
         ");
        }

        [TestMethod]
        public void Edit_RemoveVariableCall()
        {
            AssemblyUtils.Run(@"
                var a=""valA"";
                var b=a.ToString();
                var c=b.ToString();
                c=""ForceRedeclare"";
            ")

         .RunRemoveAction("a")

         .AssertSourceEquivalence(@"
                var c=""ForceRedeclare"";
         ");
        }
    }
}
