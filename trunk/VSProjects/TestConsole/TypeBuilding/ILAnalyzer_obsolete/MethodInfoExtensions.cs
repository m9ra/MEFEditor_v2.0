using System.Reflection;
using System.Text;


namespace TypeSystem.TypeBuilding.ILAnalyzer
{
    public static class MethodInfoExtensions
    {
        public static string ToPrettyString(this MethodInfo methodInfo)
        {
            return ToPrettyString(methodInfo, true);
        }


        public static string ToPrettyString(this MethodInfo methodInfo, bool useShortNotation)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(methodInfo.ReturnType.ToPrettyString(useShortNotation));
            sb.Append(" ");
            sb.Append(methodInfo.DeclaringType.ToPrettyString(useShortNotation));
            sb.Append(".");
            sb.Append(methodInfo.Name);
            sb.Append("(");
            bool isFirst = true;
            foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
            {
                if (isFirst == true) isFirst = false;
                else sb.Append(", ");

                sb.Append(parameterInfo.ToPrettyString(useShortNotation));
            }
            sb.Append(")");

            return sb.ToString();
        }
    }
}
