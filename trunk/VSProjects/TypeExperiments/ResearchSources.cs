using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnitTesting.Analyzing_TestUtils;
using UnitTesting.TypeSystem_TestUtils;

using TypeSystem;

using Analyzing;
using Analyzing.Execution;
using Analyzing.Editing;

namespace TypeExperiments
{
    static class ResearchSources
    {
        static internal TestingAssembly InstanceRemoving()
        {
            return AssemblyUtils.Run(@"
                var toDelete=""toDelete"";                
                CallWithOptional(CallWithRequired(toDelete));           
                var x=1;
                var y=2;
                if(x<y){
                    toDelete=""same"";
                }     else{
                    toDelete=""different"";
                }
            ")

         .AddMethod("CallWithOptional", (c) =>
         {
             var arg = c.CurrentArguments[1];
             c.Edits.SetOptional(1);
             c.Return(arg);
         }, "", new ParameterInfo("p", InstanceInfo.Create<string>()))

         .AddMethod("CallWithRequired", (c) =>
         {
             var arg = c.CurrentArguments[1];
             c.Return(arg);
         }, "", new ParameterInfo("p", InstanceInfo.Create<string>()))

         .AddRemoveAction("toDelete")

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
                var thisObj= c.CurrentArguments[0];
                var arg = c.CurrentArguments[1];
                c.SetField(thisObj, "inputData", arg);                

            }, "", new ParameterInfo("p", InstanceInfo.Create<string>()))

            .AddMethod("TestObj.GetInput", (c) =>
            {
                var thisObj = c.CurrentArguments[0];
                var data = c.GetField(thisObj, "inputData");
                c.Return(data);
            }, false, new ParameterInfo("p", InstanceInfo.Create<string>()))


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

").AddMethod("fib", @"    
    if(n<3){
        return 1;
    }else{
        return fib(n-1)+fib(n-2);
    }
",returnType: "System.Int32", parameters: new ParameterInfo("n", InstanceInfo.Create<int>()));
        }





        static internal TestingAssembly StaticCall()
        {
            return AssemblyUtils.Run(@"
var test=StaticClass.StaticMethod(""aaa"",153);
var test2=test;
var test3=4;

if(true){
    test3=2;
}else{
    test3=1;
}
")

 .AddMethod("StaticClass.StaticMethod", @"
        return arg1;
", true, "System.Int32",
 new ParameterInfo("arg1", InstanceInfo.Create<string>()),
 new ParameterInfo("arg2", InstanceInfo.Create<int>())
 )


 .AddMethod("StaticClass.StaticClass", @"
    return ""Initialization value"";
", true);

        }

    }
}
