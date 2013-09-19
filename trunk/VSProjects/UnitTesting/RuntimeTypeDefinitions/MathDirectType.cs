using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;

using Analyzing;

using TypeSystem;
using TypeSystem.Runtime;

namespace UnitTesting.RuntimeTypeDefinitions
{
    class MathDirectType<T> : DirectTypeDefinition<T>
        where T : IComparable
    {
        readonly Type directType;



        public MathDirectType()
        {
            directType = typeof(T);

            addDirectMath();
            addDirectComparing();
        }

        private void addDirectMath()
        {
            var add = operatorInfo("add_operator");
            var sub = operatorInfo("sub_operator");

            AddMethod(generateAddOperator(), add);
            AddMethod(generateSubOperator(), sub);
        }

        private TypeMethodInfo operatorInfo(string methodName)
        {
            var thisInfo = InstanceInfo.Create<T>();

            var op1 = TypeParameterInfo.Create("op1", thisInfo);
            var op2 = TypeParameterInfo.Create("op2", thisInfo);

            var methodInfo = new TypeMethodInfo(thisInfo, methodName, thisInfo, new TypeParameterInfo[] { op2 }, false);
            return methodInfo;
        }

        private void addDirectComparing()
        {
            var lesser = operatorInfo("lesser_operator");

            AddMethod(generateLesserThanOperator(), lesser);
        }

        private DirectMethod generateAddOperator()
        {
            var param1 = Expression.Parameter(typeof(T), "op1");
            var param2 = Expression.Parameter(typeof(T), "op2");

            return generateMathOperator(
                Expression.Add(param1, param2)
                , param1, param2
            );
        }

        private DirectMethod generateSubOperator()
        {
            var param1 = Expression.Parameter(typeof(T), "op1");
            var param2 = Expression.Parameter(typeof(T), "op2");

            return generateMathOperator(
                Expression.Subtract(param1, param2)
                , param1, param2
            );
        }

        private DirectMethod generateMathOperator(BinaryExpression mathExpression, ParameterExpression param1, ParameterExpression param2)
        {
            var addOperator = Expression.Lambda<Func<T, T, T>>(
                mathExpression,
                new ParameterExpression[] { param1, param2 }
                ).Compile();


            return (context) =>
            {
                var op1 = (T)context.CurrentArguments[0].DirectValue;
                var op2 = (T)context.CurrentArguments[1].DirectValue;
                var result = addOperator(op1, op2);
                var resultInstance = context.Machine.CreateDirectInstance(result);

                context.Return(resultInstance);
            };
        }

        private DirectMethod generateLesserThanOperator()
        {
            return (context) =>
            {
                var op1 = context.CurrentArguments[0].DirectValue as IComparable;
                var op2 = context.CurrentArguments[1].DirectValue;

                var result = context.Machine.CreateDirectInstance(op1.CompareTo(op2) < 0);
                context.Return(result);
            };
        }

    }
}
