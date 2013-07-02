using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;

using Analyzing;
using Analyzing.Execution;

namespace UnitTesting.Analyzing_TestUtils.Environment
{
    class SettingsProvider
    {
        internal static readonly MachineSettings MachineSettings;

        static SettingsProvider()
        {
            MachineSettings = new MachineSettings(
                typeof(int),typeof(string),typeof(double)
                );

            MachineSettings.AddDirectMethod(methodIdentifier<int>("+"), generateAddingOperator<int>());
        }

        private static DirectMethod generateAddingOperator<T>()
        {
            var param1 = Expression.Parameter(typeof(T), "leftOperand");
            var param2 = Expression.Parameter(typeof(T), "rightOperand");

            var addExpression=Expression.Add(param1, param2);

            var addOperator=Expression.Lambda<Func<T, T, T>>(
                addExpression,
                new ParameterExpression[] { param1, param2 }
                ).Compile();

            return (context) =>
            {
                var op1=(T)context.CurrentArguments[1].DirectValue;
                var op2=(T)context.CurrentArguments[2].DirectValue;
                var result=addOperator(op1, op2);
                var resultInstance = context.CreateDirectInstance(result);

                context.Return(resultInstance);
            };
        }

        private static string methodIdentifier<T>(string name)
        {
            return typeof(T).FullName + "." + name;
        }
    }
}
