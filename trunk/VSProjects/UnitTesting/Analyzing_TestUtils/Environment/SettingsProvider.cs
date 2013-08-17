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

            addDirectMath<int>();
            addDirectComparing<int>();
        }

        private static void addDirectMath<Type>()
        {
            var opInfo=new InstanceInfo(typeof(Type).FullName);
            var op1 = new ParameterInfo("op1", opInfo);
            var op2 = new ParameterInfo("op2", opInfo);

            TypeSettings.AddDirectMethod<Type>("add_operator".Method(),new ParameterInfo[]{op1,op2}, generateAddOperator<Type>());
            TypeSettings.AddDirectMethod<Type>("sub_operator".Method(), new ParameterInfo[] { op1, op2 }, generateSubOperator<Type>());
        }

        private static void addDirectComparing<Type>()
        {
            var opInfo = new InstanceInfo(typeof(Type).FullName);
            var op1 = new ParameterInfo("op1", opInfo);
            var op2 = new ParameterInfo("op2", opInfo);

            TypeSettings.AddDirectMethod<Type>(
                "lesser_operator".Method(),
                new ParameterInfo[] { op1, op2 },
                generateLesserThanOperator()
                );
        }

        private static DirectMethod<MethodID, InstanceInfo> generateAddOperator<T>(){
            var param1 = Expression.Parameter(typeof(T), "op1");
            var param2 = Expression.Parameter(typeof(T), "op2");

            return generateMathOperator<T>(
                Expression.Add(param1, param2)
                , param1, param2
            );
        }

        private static DirectMethod<MethodID, InstanceInfo> generateSubOperator<T>()
        {
            var param1 = Expression.Parameter(typeof(T), "op1");
            var param2 = Expression.Parameter(typeof(T), "op2");

            return generateMathOperator<T>(
                Expression.Subtract(param1, param2)
                ,param1,param2
            );
        }

        private static DirectMethod<MethodID,InstanceInfo> generateMathOperator<T>(BinaryExpression mathExpression,ParameterExpression param1, ParameterExpression param2)
        {
    

            var addOperator=Expression.Lambda<Func<T, T, T>>(
                mathExpression,
                new ParameterExpression[] { param1, param2 }
                ).Compile();
            

            return (context) =>
            {
                var op1=(T)context.CurrentArguments[0].DirectValue;
                var op2=(T)context.CurrentArguments[1].DirectValue;
                var result=addOperator(op1, op2);
                var resultInstance = context.CreateDirectInstance(result);

                context.Return(resultInstance);
            };
        }


        

        private static DirectMethod<MethodID, InstanceInfo> generateLesserThanOperator()
        {
            return (context) =>
            {
                var op1 = context.CurrentArguments[0].DirectValue as IComparable;
                var op2 = context.CurrentArguments[1].DirectValue;

                var result = context.CreateDirectInstance(op1.CompareTo(op2) <0);
                context.Return(result);
            };
        }

        private static string methodIdentifier<T>(string name)
        {
            return typeof(T).FullName + "." + name;
        }

        internal static GeneratorBase<MethodID, InstanceInfo> MethodGenerator(VersionedName methodName)
        {
            //TODO for testing purposes is this used as method initializer - Refactor
            return new DirectorGenerator((e) =>
            {
                e.AssignLiteral("result", 111);
                e.Return("result");
            });
        }

        internal static DirectAssembly CreateDirectAssembly()
        {
            var assembly = new DirectAssembly(TypeSettings);

            return assembly;
        }
    }
}
