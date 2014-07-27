using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Linq.Expressions;

using MEFEditor.Analyzing;

using MEFEditor.TypeSystem;
using MEFEditor.TypeSystem.Runtime;

namespace RecommendedExtensions.Core.Services
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
            addUnaryOperator("op_Subtraction", Expression.Negate);
            addUnaryOperator("op_Addition", Expression.UnaryPlus);
        }

        private void addDirectComparing()
        {
            addBinaryPredicate("op_LessThan", Expression.LessThan);
            addBinaryPredicate("op_GreaterThan", Expression.GreaterThan);
            addBinaryPredicate("op_LessThanOrEqual", Expression.LessThanOrEqual);
            addBinaryPredicate("op_GreaterThanOrEqual", Expression.GreaterThanOrEqual);
        }

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


        private void addBinaryPredicate(string predicateName, BinaryOperator binaryOperator)
        {
            DirectMethod directOperator;
            try
            {
                directOperator = generateDirectBinaryPredicate(binaryOperator);
            }
            catch (InvalidOperationException)
            {
                //operation is not available for desired type
                return;
            }

            addBinaryPredicate(predicateName, directOperator);
        }

        private void addBinaryOperator(string methodName, DirectMethod directOperator)
        {
            var method = binaryInfo(methodName, TypeDescriptor.Create<T>());
            AddMethod(directOperator, method);
        }

        private void addBinaryPredicate(string methodName, DirectMethod directOperator)
        {
            var method = binaryInfo(methodName, TypeDescriptor.Create<bool>());
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

            return generateDirectBinary<T>(
                binaryOperator(param1, param2)
                , param1, param2
            );
        }

        private DirectMethod generateDirectBinaryPredicate(BinaryOperator binaryOperator)
        {
            var param1 = Expression.Parameter(typeof(T), "op1");
            var param2 = Expression.Parameter(typeof(T), "op2");

            return generateDirectBinary<bool>(
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

        private DirectMethod generateDirectBinary<TResult>(BinaryExpression mathExpression, ParameterExpression param1, ParameterExpression param2)
        {
            var directOperator = Expression.Lambda<Func<T, T, TResult>>(
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

        private TypeMethodInfo binaryInfo(string methodName, TypeDescriptor resultInfo)
        {
            var thisInfo = TypeDescriptor.Create<T>();

            var op1 = ParameterTypeInfo.Create("op1", thisInfo);
            var op2 = ParameterTypeInfo.Create("op2", thisInfo);

            var methodInfo = new TypeMethodInfo(thisInfo, methodName, resultInfo, new ParameterTypeInfo[] { op2 }, false, TypeDescriptor.NoDescriptors);
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
