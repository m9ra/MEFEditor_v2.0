using System;
using System.Reflection;


namespace TypeSystem.TypeBuilding.ILAnalyzer
{
    public static class MethodBaseExtensions
    {
        public static string ToPrettyString(this MethodBase methodBase)
        {
            return ToPrettyString(methodBase, true);
        }


        public static string ToPrettyString(this MethodBase methodBase, bool useShortNotation)
        {
            if ((methodBase is MethodInfo) == true) return (methodBase as MethodInfo).ToPrettyString(useShortNotation);
            if ((methodBase is ConstructorInfo) == true) return (methodBase as ConstructorInfo).ToPrettyString(useShortNotation);

            throw new ArgumentException();
        }
    }
}
