using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Linq.Expressions;

using Analyzing;

using TypeSystem;
using TypeSystem.Runtime;

namespace AssemblyProviders.DirectDefinitions
{

    delegate BinaryExpression BinaryOperator(ParameterExpression op1, ParameterExpression op2);

    delegate UnaryExpression UnaryOperator(ParameterExpression op);

    public class MathDirectType<T> : DirectTypeDefinition<T>
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
            addBinaryOperator("op_Addition", Expression.Add);
            addBinaryOperator("op_Subtraction", Expression.Subtract);
            addBinaryOperator("op_Multiply", Expression.Multiply);
            addBinaryOperator("op_Division", Expression.Divide);
            addBinaryOperator("op_Modulus", Expression.Modulo);

            addBinaryOperator("op_BitwiseAnd", Expression.And);
            addBinaryOperator("op_BitwiseOr", Expression.Or);
            addBinaryOperator("op_BitwiseAnd", Expression.AndAlso);
            addBinaryOperator("op_BitwiseOr", Expression.OrElse);

            addUnaryOperator("op_Not", Expression.Not);
            addUnaryOperator("op_UnaryNegation", Expression.Negate);
            addUnaryOperator("op_UnaryPlus", Expression.UnaryPlus);
        }

        private void addDirectComparing()
        {
            addBinaryOperator("op_LessThan", generateLesserThanOperator());
        }

        #region Operators implementation

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

        #endregion


        #region Utility methods

        private void addBinaryOperator(string methodName, BinaryOperator binaryOperator)
        {
            DirectMethod directOperator;
            try
            {
                directOperator = generateDirectBinaryOperator(binaryOperator);
            }
            catch (InvalidOperationException)
            {
                //operation is not available for desired type
                return;
            }

            addBinaryOperator(methodName, directOperator);
        }

        private void addBinaryOperator(string methodName, DirectMethod directOperator)
        {
            var method = binaryOperatorInfo(methodName);
            AddMethod(directOperator, method);
        }

        private void addUnaryOperator(string methodName, UnaryOperator unaryOperator)
        {
            DirectMethod directOperator;
            try
            {
                directOperator = generateDirectUnaryOperator(unaryOperator);
            }
            catch (InvalidOperationException)
            {
                //operation is not available for desired type
                return;
            }

            var method = unaryOperatorInfo(methodName);
            AddMethod(directOperator, method);
        }


        private DirectMethod generateDirectBinaryOperator(BinaryOperator binaryOperator)
        {
            var param1 = Expression.Parameter(typeof(T), "op1");
            var param2 = Expression.Parameter(typeof(T), "op2");

            return generateMathOperator(
                binaryOperator(param1, param2)
                , param1, param2
            );
        }

        private DirectMethod generateDirectUnaryOperator(UnaryOperator unaryOperator)
        {
            var param1 = Expression.Parameter(typeof(T), "op");

            return generateMathOperator(
                unaryOperator(param1)
                , param1
            );
        }

        private DirectMethod generateMathOperator(BinaryExpression mathExpression, ParameterExpression param1, ParameterExpression param2)
        {
            var directOperator = Expression.Lambda<Func<T, T, T>>(
                mathExpression,
                new ParameterExpression[] { param1, param2 }
                ).Compile();


            return (context) =>
            {
                var op1 = (T)context.CurrentArguments[0].DirectValue;
                var op2 = (T)context.CurrentArguments[1].DirectValue;
                var result = directOperator(op1, op2);
                var resultInstance = context.Machine.CreateDirectInstance(result);

                context.Return(resultInstance);
            };
        }

        private DirectMethod generateMathOperator(UnaryExpression mathExpression, ParameterExpression param)
        {
            var directOperator = Expression.Lambda<Func<T, T>>(
                mathExpression,
                new ParameterExpression[] { param }
                ).Compile();


            return (context) =>
            {
                var op = (T)context.CurrentArguments[0].DirectValue;
                var result = directOperator(op);
                var resultInstance = context.Machine.CreateDirectInstance(result);

                context.Return(resultInstance);
            };
        }

        private TypeMethodInfo binaryOperatorInfo(string methodName)
        {
            var thisInfo = TypeDescriptor.Create<T>();

            var op1 = ParameterTypeInfo.Create("op1", thisInfo);
            var op2 = ParameterTypeInfo.Create("op2", thisInfo);

            var methodInfo = new TypeMethodInfo(thisInfo, methodName, thisInfo, new ParameterTypeInfo[] { op2 }, false, TypeDescriptor.NoDescriptors);
            return methodInfo;
        }


        private TypeMethodInfo unaryOperatorInfo(string methodName)
        {
            var thisInfo = TypeDescriptor.Create<T>();

            var op = ParameterTypeInfo.Create("op", thisInfo);

            var methodInfo = new TypeMethodInfo(thisInfo, methodName, thisInfo, ParameterTypeInfo.NoParams, false, TypeDescriptor.NoDescriptors);
            return methodInfo;
        }

        #endregion
    }
}
