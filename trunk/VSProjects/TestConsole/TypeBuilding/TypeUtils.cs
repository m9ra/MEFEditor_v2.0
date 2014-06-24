using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using TypeExperiments.Core;

namespace TypeExperiments.TypeBuilding
{
    static class TypeUtils
    {
        public static MethodInfo GetMethod(Type type,string methodName, params Instance[] args)
        {
            throw new NotImplementedException();
        }

        public static MethodInfo GetMethod(Type type,string methodName, params Type[] argTypes)
        {
            throw new NotImplementedException();
        }

        public static ConstructorInfo GetConstructor(Type type, params Instance[] args)
        {
            throw new NotImplementedException();
        }
        public static ConstructorInfo GetConstructor(Type type, params Type[] argTypes)
        {
            throw new NotImplementedException();
        }

        public static Type[] TypesFromObjs(params object[] obj)
        {
            throw new NotImplementedException();
        }
    }
}
