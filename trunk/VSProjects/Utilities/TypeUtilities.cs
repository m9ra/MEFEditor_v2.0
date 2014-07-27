using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;

namespace Utilities
{
    /// <summary>
    /// Class of utility methods on <see cref="Type"/>.
    /// </summary>
    public static class TypeUtilities
    {
        /// <summary>
        /// Utility method for dynamic casting of types
        /// Taken from: http://stackoverflow.com/questions/7351289/how-do-i-perform-explicit-operation-casting-from-reflection.
        /// </summary>
        /// <param name="source">Object to be casted.</param>
        /// <param name="destType">Desired type.</param>
        /// <returns>Casted object.</returns>
        /// <exception cref="System.InvalidCastException"></exception>
        public static object DynamicCast(object source, Type destType)
        {
            Type srcType = source.GetType();
            if (srcType == destType) return source;
            if (destType.IsAssignableFrom(srcType)) return source;

            var paramTypes = new Type[] { srcType };
            MethodInfo cast = destType.GetMethod("op_Implicit", paramTypes);

            if (cast == null)
            {
                cast = destType.GetMethod("op_Explicit", paramTypes);
            }

            if (cast == null && destType==typeof(string))
            {
                return source.ToString();
            }

            if (cast == null)
            {
                if (destType.IsEnum) return Enum.ToObject(destType, source);
                throw new InvalidCastException();
            }

            return cast.Invoke(null, new object[] { source });
        }
    }
}
