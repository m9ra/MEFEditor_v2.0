using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Analyzing.Execution.Instructions;
using UnitTesting.Analyzing_TestUtils;

using Analyzing;
using TypeSystem;

namespace UnitTesting
{
    [TestClass]
    public class Machine_Testing
    {
        [TestMethod]
        public void Assign()
        {
            ExecutionUtils.Run((e) =>
            {
                e.AssignLiteral("var1", 1);
                e.Assign("var2", "var1");
                e.Assign("var1", "var1");
            })
            .AssertVariable("var1").HasValue(1)
            .AssertVariable("var2").HasValue(1);
        }

        [TestMethod]
        public void DirectCall()
        {
            var toLower = Naming.Method<string>("ToLower");

            ExecutionUtils.Run((e) =>
            {
                e.AssignLiteral("var1", "HELLO");
                e.Call(toLower, "var1", Arguments.Values());
                e.AssignReturnValue("var2", InstanceInfo.Create<string>());
            })
            .AssertVariable("var2").HasValue("hello");
        }

        [TestMethod]
        public void DirectCall_withArguments()
        {
            var toString = Naming.Method<int>("ToString", typeof(string));

            ExecutionUtils.Run((e) =>
            {
                e.AssignLiteral("var1", 25);
                e.AssignLiteral("format", "Number: {0}");
                e.Call(toString, "var1", Arguments.Values("format"));
                e.AssignReturnValue("var2", InstanceInfo.Create<string>());
            })
            .AssertVariable("var2").HasValue(25.ToString("Number: {0}"));
        }

        [TestMethod]
        public void InjectedMethodCall_adding()
        {
            var add = Naming.Method<int>("add_operator", typeof(int));

            ExecutionUtils.Run((e) =>
            {
                e.AssignLiteral("var1", 40);
                e.AssignLiteral("var2", 2);
                e.Call(add, "var1", Arguments.Values("var2"));
                e.AssignReturnValue("var3", InstanceInfo.Create<string>());
            })
            .AssertVariable("var3").HasValue(40 + 2);
        }

        [TestMethod]
        public void ConditionalLoop_iteration()
        {
            var add = Naming.Method<int>("add_operator", typeof(int));
            var equals = Naming.Method<int>("Equals", typeof(int));

            ExecutionUtils.Run((e) =>
            {
                var start = e.CreateLabel("start");
                var end = e.CreateLabel("end");

                e.AssignLiteral("stop", 100);
                e.AssignLiteral("step", 1);
                e.AssignLiteral("increment", 0);

                e.SetLabel(start);
                e.Call(add, "increment", Arguments.Values("step"));
                e.AssignReturnValue("increment", InstanceInfo.Create<int>());
                e.Call(equals, "increment", Arguments.Values("stop"));
                e.AssignReturnValue("condition", InstanceInfo.Create<bool>());
                e.ConditionalJump("condition", end);
                e.Jump(start);
                e.SetLabel(end);
            })
            .AssertVariable("increment").HasValue(100);
        }


    }
}
