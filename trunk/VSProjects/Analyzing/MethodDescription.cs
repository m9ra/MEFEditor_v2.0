using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing
{
    public class MethodDescription
    {
        public readonly TypeDescription ThisType;

        public readonly string MethodName;

        public readonly bool IsStatic;

        public readonly ParamDescription[] Parameters;
        

        public MethodDescription(TypeDescription thisType, string methodName, ParamDescription[] parameters, bool isStatic)
        {
            ThisType = thisType;
            MethodName = methodName;  
            Parameters = parameters;
            IsStatic = isStatic;
        }
    }
}
