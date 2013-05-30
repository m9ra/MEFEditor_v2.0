using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;


using Analyzing.Execution.Instructions;
using UnitTesting.Analyzing_TestUtils;


namespace UnitTesting
{
    [TestClass]
    public class Machine_Testing
    {
        [TestMethod]
        public void AssignTest()
        {
            ExecutionUtils.Run((e) =>
            {
                e.AssignLiteral("var1", 1);
                e.Assign("var2", "var1");
                e.Assign("var1", "var1");
            });
        }
    }
}
