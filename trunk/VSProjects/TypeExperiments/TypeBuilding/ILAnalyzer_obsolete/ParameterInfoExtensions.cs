using System.Reflection;


namespace TypeSystem.TypeBuilding.ILAnalyzer
{
    public static class ParameterInfoExtensions
    {
        public static string ToPrettyString(this ParameterInfo parameterInfo)
        {
            return ToPrettyString(parameterInfo, true);
        }



        public static string ToPrettyString(this ParameterInfo parameterInfo, bool useShortNotation)
        {
            return parameterInfo.ParameterType.ToPrettyString(useShortNotation);
        }
    }
}
