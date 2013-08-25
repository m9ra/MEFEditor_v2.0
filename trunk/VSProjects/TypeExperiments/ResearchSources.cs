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

        static Instance EXTERNAL_INPUT;

        static internal TestingAssembly EditProvider()
        {
            return AssemblyUtils.Run(@"
                var arg=""input"";
                DirectMethod(""input2"");

            ")

             .AddMethod("DirectMethod", (c) =>
             {
                 var thisInst = c.CurrentArguments[0];
                 var arg = c.CurrentArguments[1];
                 c.Edits.AppendArgument(thisInst, ".accept", (t) => acceptInstance(c.Edits, t));
                 //c.Edits.RemoveArgument(arg, 1, ".reject");
                 //     c.Edits.ChangeArgument(thisInst, 1, "Set new Value", () => "new value");

                 var res = c.CreateDirectInstance("Direct result");
                 c.Return(res);

             }, false, new ParameterInfo("p", new InstanceInfo("System.String")))

            .UserAction((c)=>{
                EXTERNAL_INPUT = c.EntryContext.GetValue(new VariableName("arg"));
            })

            .AddEditAction("this", ".accept");
        }


        static object acceptInstance(EditsProvider<MethodID,InstanceInfo> edits, TransformationServices services)
        {
            var variable = edits.GetVariableFor(EXTERNAL_INPUT,services);
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
", parameters: new ParameterInfo("n", new InstanceInfo("System.Int32")));
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
", true,
 new ParameterInfo("arg1", new InstanceInfo("System.String")),
 new ParameterInfo("arg2", new InstanceInfo("System.Int32"))
 )


 .AddMethod("StaticClass.StaticClass", @"
    return ""Initialization value"";
", true);

        }

    }
}
