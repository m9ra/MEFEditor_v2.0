using System;
using System.Text;


namespace TypeSystem.TypeBuilding.ILAnalyzer
{
    public static class TypeExtensions
    {
        public static string ToPrettyString(this Type type)
        {
            return ToPrettyString(type, true);
        }


        public static string ToPrettyString(this Type type, bool useShortNotation)
        {
            if (type.IsGenericType == false)
            {
                if (useShortNotation == true)
                {
                    if (type == typeof(int)) return "int";
                    else if (type == typeof(void)) return "void";
                    else if (type == typeof(object)) return "object";
                    else if (type == typeof(string)) return "string";                    
                }

                return type.FullName;
            }
            else
            {
                Type genericType = type.GetGenericTypeDefinition();

                StringBuilder sb = new StringBuilder();

                sb.Append(genericType.FullName);
                sb.Append("[");
                bool isFirst = true;
                foreach (Type parameterType in type.GetGenericArguments())
                {
                    if (isFirst == true) isFirst = false;
                    else sb.Append(", ");

                    sb.Append(parameterType.ToPrettyString());
                }
                sb.Append("]");

                return sb.ToString();
            }
        }
    }
}
