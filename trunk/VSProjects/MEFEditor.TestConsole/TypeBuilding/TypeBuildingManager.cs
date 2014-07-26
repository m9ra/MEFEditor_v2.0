using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeExperiments.Core;
using TypeExperiments.Reflection;
using TypeExperiments.TypeBuilding;

using System.Reflection;

namespace TypeExperiments.TypeBuilding
{
    static class TypeBuildingManager
    {
        /// <summary>
        /// Creates high performance representation of given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static InternalType WrapType(Type type)
        {
            var name = getName(type);
            var baseName = getName(type.BaseType);
            var builder = createBuilder(name, baseName);

            var fields = type.GetFields();
            AddFields(fields, builder);

            var methods = getAllMethods(type);
            AddMethods(methods, builder);

            return builder.CreateType();
        }

        private static void AddMethods(IEnumerable<MethodInfo> methods, InternalTypeBuilder builder)
        {
            foreach (var method in methods)
            {
                var methodDefinition = MethodBuilder.Wrap(method);
                builder.Add(methodDefinition);
            }
        }

        private static void AddFields(IEnumerable<FieldInfo> fields, InternalTypeBuilder builder)
        {
            throw new NotImplementedException();
        }

        private static TypeName getName(Type type)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<MethodInfo> getAllMethods(Type type)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        }


        /// <summary>
        /// Creates builder which will be used for building type of given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static InternalTypeBuilder createBuilder(TypeName name, TypeName baseName)
        {
            return new InternalTypeBuilder(name, baseName);
        }
    }
}
