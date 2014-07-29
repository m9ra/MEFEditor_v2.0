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

    /// <summary>
    /// <see cref="DirectTypeDefinition" /> implementation that enhance given type about mathematical operators
    /// with naming expected by <see cref="MEFEditor.TypeSystem" />. Operators are implemented by strongly typed
    /// native methods because of performance purposes.
    /// </summary>
    /// <typeparam name="T">The direct type.</typeparam>
    public class MathDirectType<T> : DirectTypeDefinition<T>
            where T : IComparable
    {
        /// <summary>
        /// The direct type that is represented by current direct type definition.
        /// </summary>
        readonly Type directType;

        /// <summary>
        /// Initializes a new instance of the <see cref="MathDirectType{T}" /> class.
        /// </summary>
        public MathDirectType()
        {
            directType = typeof(T);

            addDirectMath();
            addDirectComparing();
        }

        /// <summary>
        /// Add direct methods for math operators.
        /// </summary>
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

        /// <summary>
        /// Adds direct methods with comparisons.
        /// </summary>
        private void addDirectComparing()
        {
            addBinaryPredicate("op_LessThan", Expression.LessThan);
            addBinaryPredicate("op_GreaterThan", Expression.GreaterThan);
            addBinaryPredicate("op_LessThanOrEqual", Expression.LessThanOrEqual);
            addBinaryPredicate("op_GreaterThanOrEqual", Expression.GreaterThanOrEqual);
        }

        #region Utility methods

        /// <summary>
        /// Adds the binary operator.
        /// </summary>
        /// <param name="methodName">Name of the operator method.</param>
        /// <param name="binaryOperator">The binary operator.</param>
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


        /// <summary>
        /// Adds the binary predicate.
        /// </summary>
        /// <param name="predicateName">Name of the predicate.</param>
        /// <param name="binaryOperator">The binary operator.</param>
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

        /// <summary>
        /// Adds the binary operator.
        /// </summary>
        /// <param name="methodName">Name of the operator method.</param>
        /// <param name="directOperator">The direct operator.</param>
        private void addBinaryOperator(string methodName, DirectMethod directOperator)
        {
            var method = binaryInfo(methodName, TypeDescriptor.Create<T>());
            AddMethod(directOperator, method);
        }

        /// <summary>
        /// Adds the binary predicate.
        /// </summary>
        /// <param name="methodName">Name of the operator method.</param>
        /// <param name="directOperator">The direct operator.</param>
        private void addBinaryPredicate(string methodName, DirectMethod directOperator)
        {
            var method = binaryInfo(methodName, TypeDescriptor.Create<bool>());
            AddMethod(directOperator, method);
        }

        /// <summary>
        /// Adds the unary operator.
        /// </summary>
        /// <param name="methodName">Name of the operator method.</param>
        /// <param name="unaryOperator">The unary operator.</param>
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


        /// <summary>
        /// Generates the direct binary operator.
        /// </summary>
        /// <param name="binaryOperator">The binary operator.</param>
        /// <returns>DirectMethod.</returns>
        private DirectMethod generateDirectBinaryOperator(BinaryOperator binaryOperator)
        {
            var param1 = Expression.Parameter(typeof(T), "op1");
            var param2 = Expression.Parameter(typeof(T), "op2");

            return generateDirectBinary<T>(
                binaryOperator(param1, param2)
                , param1, param2
            );
        }

        /// <summary>
        /// Generates the direct binary predicate.
        /// </summary>
        /// <param name="binaryOperator">The binary operator.</param>
        /// <returns>DirectMethod.</returns>
        private DirectMethod generateDirectBinaryPredicate(BinaryOperator binaryOperator)
        {
            var param1 = Expression.Parameter(typeof(T), "op1");
            var param2 = Expression.Parameter(typeof(T), "op2");

            return generateDirectBinary<bool>(
                binaryOperator(param1, param2)
                , param1, param2
            );
        }

        /// <summary>
        /// Generates the direct unary operator.
        /// </summary>
        /// <param name="unaryOperator">The unary operator.</param>
        /// <returns>DirectMethod.</returns>
        private DirectMethod generateDirectUnaryOperator(UnaryOperator unaryOperator)
        {
            var param1 = Expression.Parameter(typeof(T), "op");

            return generateMathOperator(
                unaryOperator(param1)
                , param1
            );
        }

        /// <summary>
        /// Generates the direct binary operator.
        /// </summary>
        /// <typeparam name="TResult">The type of the t result.</typeparam>
        /// <param name="mathExpression">The math expression.</param>
        /// <param name="param1">The param1.</param>
        /// <param name="param2">The param2.</param>
        /// <returns>DirectMethod.</returns>
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

        /// <summary>
        /// Generates the math operator.
        /// </summary>
        /// <param name="mathExpression">The math expression.</param>
        /// <param name="param">The parameter.</param>
        /// <returns>DirectMethod.</returns>
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

        /// <summary>
        /// Generate binary info for specified method.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="resultInfo">The result information.</param>
        /// <returns>TypeMethodInfo.</returns>
        private TypeMethodInfo binaryInfo(string methodName, TypeDescriptor resultInfo)
        {
            var thisInfo = TypeDescriptor.Create<T>();

            var op1 = ParameterTypeInfo.Create("op1", thisInfo);
            var op2 = ParameterTypeInfo.Create("op2", thisInfo);

            var methodInfo = new TypeMethodInfo(thisInfo, methodName, resultInfo, new ParameterTypeInfo[] { op2 }, false, TypeDescriptor.NoDescriptors);
            return methodInfo;
        }


        /// <summary>
        /// Generate unary info for specified method.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <returns>TypeMethodInfo.</returns>
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
