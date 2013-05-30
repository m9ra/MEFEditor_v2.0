using System.Reflection;
using System.Text;


namespace TypeSystem.TypeBuilding.ILAnalyzer
{
    public static class ConstructorInfoExtensions
    {
        public static string ToPrettyString(this ConstructorInfo constructorInfo)
        {
            return ToPrettyString(constructorInfo, true);
        }



        public static string ToPrettyString(this ConstructorInfo constructorInfo, bool useShortNotation)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(constructorInfo.DeclaringType.ToPrettyString(useShortNotation));
            sb.Append(constructorInfo.Name);
            sb.Append("(");
            bool isFirst = true;
            foreach (ParameterInfo parameterInfo in constructorInfo.GetParameters())
            {
                if (isFirst == true) isFirst = false;
                else sb.Append(", ");

                sb.Append(parameterInfo.ParameterType.ToPrettyString(useShortNotation));
                sb.Append(" ");
                sb.Append(parameterInfo.Name);
            }
            sb.Append(")");

            return sb.ToString();
        }
    }
}
