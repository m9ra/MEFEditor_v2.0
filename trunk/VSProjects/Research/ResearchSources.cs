using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnitTesting;
using UnitTesting.RuntimeTypeDefinitions;
using UnitTesting.Analyzing_TestUtils;
using UnitTesting.TypeSystem_TestUtils;

using TypeSystem;
using TypeSystem.Runtime;

using Analyzing;
using Analyzing.Execution;
using Analyzing.Editing;

using MEFAnalyzers;


namespace TypeExperiments
{
    static class ResearchSources
    {


        static internal TestingAssembly GenericTesting()
        {
            return AssemblyUtils.Run(@"                
                   var list=new System.Collections.Generic.List<System.String>();     
                   list.Add(""test"");

                   var result=list[0];
               ")

        //    .AddGenericToRuntime(typeof(List<>))

            ;
        }

        static internal TestingAssembly ExplicitGenericTesting()
        {
            return AssemblyUtils.Run(@"                
                   var list=new System.Collections.Generic.List<System.String>();     
                   list.Add(""test"");

                   var result=list[0];
               ")

            .AddDirectToRuntime<List<string>>()

            ;
        }

        static internal TestingAssembly ComplexDirectRuntime()
        {
            return AssemblyUtils.Run(@"                
                var test=new System.Text.StringBuilder();
                test.Append(""Data"");
                test.Append(""2"");
                var result=test.ToString();      
            ")

            .AddDirectToRuntime<StringBuilder>()

            ;
        }

        static internal TestingAssembly CompositionTester()
        {
            return AssemblyUtils.Run(@"        
                var partImport=new StringImport();       
                var partExport=new StringExport(""ExportedValue"");

                var test=new CompositionTester(partImport,partExport);   
                var importValue=partImport.Import;                   
            ")

            .AddToRuntime<CompositionTesterDefinition>()
            .AddToRuntime<StringImport>()
            .AddToRuntime<StringExport>()
            ;
        }

        static internal TestingAssembly RuntimeCall_Default()
        {
            return AssemblyUtils.Run(@"                
                var test=new SimpleType(""CtorValue"");      
                var result=test.Concat();      
            ")

           .AddToRuntime<SimpleType>()

           ;
        }

        static internal TestingAssembly InstanceRemoving()
        {
            return AssemblyUtils.Run(@"
                var toDelete=""toDelete"";                
                CallWithOptional(CallWithRequired(toDelete));           
                var x=1;
                var y=2;
                if(x<y){
                    toDelete=""smaller"";
                }else{
                    toDelete=""greater"";
                }
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

          ;

        }

        static internal TestingAssembly StaticCall()
        {
            return AssemblyUtils.Run(@"
                var test=StaticClass.StaticMethod(""CallArg"");
            ")

            .AddMethod("StaticClass.StaticMethod", (c) =>
            {
                var self = c.CurrentArguments[0];
                var arg = c.CurrentArguments[1].DirectValue as string;
                var field = c.GetField(self, "StaticField");

                var result = c.Machine.CreateDirectInstance(field + "_" + arg);
                c.Return(result);
            }, Method.StaticString_StringParam)

            .AddMethod("StaticClass.#initializer", (c) =>
            {
                var self = c.CurrentArguments[0];
                c.SetField(self, "StaticField", "InitValue");
            }, Method.StaticInitializer)

             ;
        }

        static internal TestingAssembly EditProvider()
        {
            return AssemblyUtils.Run(@"
                var obj=new TestObj(""input"");
                
                var result = obj.GetInput();          
            ")

            .AddMethod("TestObj.TestObj", (c) =>
            {
                var thisObj = c.CurrentArguments[0];
                var arg = c.CurrentArguments[1];
                c.SetField(thisObj, "inputData", arg);

            }, Method.Ctor_StringParam)

            .AddMethod("TestObj.GetInput", (c) =>
            {
                var thisObj = c.CurrentArguments[0];
                var data = c.GetField(thisObj, "inputData") as Instance;
                c.Return(data);
            }, Method.String_NoParam)


            ;
        }

        static object acceptInstance(EditsProvider edits, TransformationServices services)
        {
            var variable = edits.GetVariableFor(AssemblyUtils.EXTERNAL_INPUT, services);
            if (variable == null)
            {
                return services.Abort("Cannot get variable for instance");
            }

            return variable;
        }

        static internal TestingAssembly Fibonacci(int n)
        {
            return AssemblyUtils.Run(@"
                var result=fib(" + n + @");
            ")

            .AddMethod("Test.fib", @"    
                if(n<3){
                    return 1;
                }else{
                    return fib(n-1)+fib(n-2);
                }
            ", Method.Int_IntParam);
        }
    }
}
