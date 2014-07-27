using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecommendedExtensions.Core.AssemblyProviders.ProjectAssembly
{
    /// <summary>
    /// Class used for working with operators.
    /// </summary>
    public class OperatorTools
    {
        /// <summary>
        /// Table for operator to method name, all binary operators has _b suffix
        /// </summary>
        static Dictionary<string, string> _opTable = new Dictionary<string, string>{
            {"+_b","op_Addition"},
            {"-_b","op_Subtraction"},
            {"*_b","op_Multiply"},
            {"/_b","op_Division"},
            {"%_b","op_Modulus"},
            {"^_b","op_ExclusiveOr"},
            {"&_b","op_BitwiseAnd"},
            {"|_b","op_BitwiseOr"},
            {"&&_b","op_LogicalAnd"},
            {"||_b","op_LogicalOr"},
            {"=","op_Assign"},
            {"<<_b","op_LeftShift"},
            {">>_b","op_RightShift"},
            {"==_b","op_Equality"},
            {">_b","op_GreaterThan"},
            {"<_b","op_LessThan"},
            {"!=_b","op_Inequality"},
            {">=_b","op_GreaterThanOrEqual"},
            {"<=_b","op_LessThanOrEqual"},
            {"*=","op_MultiplicationAssignment"},
            {"-=","op_SubtractionAssignment"},
            {"^=","op_ExclusiveOrAssignment"},
            {"<<=","op_LeftShiftAssignment"},
            {"%=","op_ModulusAssignment"},
            {"+=","op_AdditionAssignment"},
            {"&=","op_BitwiseAndAssignment"},
            {"|=","op_BitwiseOrAssignment"},
            {",","op_Comma"},
            {"/=","op_DivisionAssignment"},
            {"--","op_Decrement"},
            {"++","op_Increment"},
            {"-","op_UnaryNegation"},
            {"+","op_UnaryPlus"},
            {"~","op_OnesComplement"}
        };
        /// <summary>
        /// Find method name, used for opSign binary operator representation.
        /// </summary>
        /// <param name="opSign">Sign of binary operator (e.g. =,+,..)</param>
        /// <returns>Method name representation of given operator. If no representation is found, returns null.</returns>
        public static string GetBinaryOperatorMethod(string opSign)
        {
            string result;
            _opTable.TryGetValue(opSign + "_b", out result);
            return result;
        }

        /// <summary>
        /// Find method name, used for opSign unary operator representation.
        /// </summary>
        /// <param name="opSign">Sign of unary operator (e.g. ++,--,..)</param>
        /// <returns>Method name representation of given operator. If no representation is found, returns null.</returns>
        public static string GetUnaryOperatorMethod(string opSign)
        {
            string result;
            _opTable.TryGetValue(opSign, out result);
            return result;
        }
    }
}
