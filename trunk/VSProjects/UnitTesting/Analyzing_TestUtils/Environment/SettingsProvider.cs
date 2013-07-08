using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;

using Analyzing;
using Analyzing.Execution;

using TypeSystem;

namespace UnitTesting.Analyzing_TestUtils.Environment
{
    class SettingsProvider
    {
        internal static readonly TypeSystem.Settings TypeSettings;
        internal static readonly TypeSystem.MachineSettings MachineSettings;

        static SettingsProvider()
        {
            MachineSettings = new MachineSettings();

            TypeSettings = new TypeSystem.Settings(
                typeof(int),typeof(string),typeof(double),typeof(bool)
                );

            addDirectAdding<int>();            
        }

        private static void addDirectAdding<Type>()
        {
            var operand = new InstanceInfo(typeof(Type).FullName);
            TypeSettings.AddDirectMethod<Type>("+".Method(),new InstanceInfo[]{operand,operand}, generateAddingOperator<Type>());
        }

        private static DirectMethod<MethodID, InstanceInfo> generateAddingOperator<T>()
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

        internal static IInstructionGenerator<MethodID, InstanceInfo> MethodGenerator(VersionedName methodName)
        {
            //TODO for testing purposes is this used as method initializer - Refactor
            return new DirectorGenerator((e) =>
            {
                e.AssignLiteral("result", 111);
                e.Return("result");
            });
        }
    }
}
