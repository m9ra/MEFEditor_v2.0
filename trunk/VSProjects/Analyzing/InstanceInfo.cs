using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing
{
    public class InstanceInfo
    {
        public readonly string TypeName;

        public static readonly InstanceInfo Void = new InstanceInfo(typeof(void));

        public InstanceInfo(string typeName)
        {
            if (typeName == null || typeName == "")
            {
                throw new NotSupportedException("Unsupported typename: " + typeName);
            }
            TypeName = typeName;
        }


        public InstanceInfo(Type type)
        {
            if (type.IsGenericType)
            {
                TypeName = GenericTypeName(type);
            }
            else
            {
                TypeName = type.FullName;
            }
        }

        public static string GenericTypeName(Type genericType, Queue<Type> genericArguments = null)
        {
            if (genericArguments == null)
            {
                //initialize generic arguments used for generic parameters substitution
                genericArguments = new Queue<Type>(genericType.GetGenericArguments());
            }

            if (genericType.IsGenericParameter)
            {
                //argument doesn't need stack with outer arguments
                return GenericTypeName(genericArguments.Dequeue());
            }

            var result = typeNamePrefix(genericType, genericArguments);

            var name = genericType.Name;
            if (!genericType.IsGenericType)
            {
                //there are no generic arguments
                result.Append(name);
            }
            else
            {
                //repair generic name
                var nameEnding = name.IndexOf('`');                
                result.Append(name.Substring(0, nameEnding));

                //handle generic arguments
                result.Append('<');
                foreach (var genericArg in genericArguments)
                {
                    result.Append(GenericTypeName(genericArg));
                    result.Append(',');
                }
                //overwrite last coma
                result[result.Length - 1] = '>';
            }

            return result.ToString();
        }

        private static StringBuilder typeNamePrefix(Type genericType, Queue<Type> genericArguments)
        {
            var result = new StringBuilder();
            if (genericType.DeclaringType != null)
            {
                //there is an outer declaring type - it preceeds it's name in notation
                var declaringType = genericType.DeclaringType;
                var declaringArgumentsCount=declaringType.GetGenericArguments().Length;
                var substitutedArguments = genericArguments.Take(declaringArgumentsCount);
                var substitutingQueue = new Queue<Type>(substitutedArguments);
                
                //prefix with declaring type name
                result.Append(GenericTypeName(declaringType, substitutingQueue));
                result.Append('.');

                //remove substituted arguments
                for (int i = 0; i < declaringArgumentsCount; ++i) genericArguments.Dequeue();
            }

            result.Append(genericType.Namespace);
            result.Append('.');
            return result;
        }

        public static InstanceInfo Create<Type>()
        {
            return new InstanceInfo(typeof(Type));
        }

        public override string ToString()
        {
            return "[InstanceInfo]" + TypeName;
        }


        public override int GetHashCode()
        {
            return TypeName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var o = obj as InstanceInfo;
            if (o == null)
            {
                return false;
            }
            return TypeName.Equals(o.TypeName);
        }
    }
}
