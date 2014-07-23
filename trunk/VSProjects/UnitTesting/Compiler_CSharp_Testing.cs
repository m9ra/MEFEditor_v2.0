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
    /// Testing of C# Lexer, Parser and Compiler from Recommended Extensions.
    /// </summary>
    [TestClass]
    public class Compiler_CSharp_Testing
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

            .AddMethod("TestObj." + Naming.CtorName, @"
                Property=""ValueToSet"";                
            ", Method.Ctor_NoParam)

            .AddMethod("TestObj.#initializer", (c) => { }, Method.Void_NoParam)

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
        public void Compile_Indexer_Setter()
        {
            AssemblyUtils.Run(@"
                var obj=new TestObj();
                obj[""a"",""b""]=""c"";
                var result=obj.Property;
            ")

            .AddMethod("TestObj.#ctor", (c) => { }, Method.Ctor_NoParam)

            .AddMethod("TestObj.set_Item", (c) =>
            {
                var arg1 = c.CurrentArguments[1].DirectValue as string;
                var arg2 = c.CurrentArguments[2].DirectValue as string;
                var arg3 = c.CurrentArguments[3].DirectValue as string;
                var thisObj = c.CurrentArguments[0];

                c.SetField(thisObj, "_property", arg1 + arg2 + arg3);

            }, Method.Void_StringStringStringParam)

            .AddMethod("TestObj.get_Property", (c) =>
            {
                var thisObj = c.CurrentArguments[0];
                var data = c.GetField(thisObj, "_property") as string;
                c.Return(c.Machine.CreateDirectInstance(data));
            }, Method.String_NoParam)

            .AssertVariable("result").HasValue("abc");
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

            .AddMethod("Test." + Naming.CtorName, (c) => { }, Method.Ctor_NoParam)

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
    }
}
