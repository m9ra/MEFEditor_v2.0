using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using Analyzing;

namespace TypeSystem
{
    public class TypeParameterInfo
    {
        public readonly string Name;
        public readonly InstanceInfo Type;
        public readonly object DefaultValue;
        public readonly bool HasDefaultValue;

        private TypeParameterInfo(string name, InstanceInfo type, object defaultValue, bool hasDefaultValue)
        {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
            HasDefaultValue = hasDefaultValue;

        }

        public static TypeParameterInfo Create(string name, InstanceInfo type)
        {
            return new TypeParameterInfo(name, type, null, false);
        }

        public static TypeParameterInfo CreateWithDefault(string name, InstanceInfo type, object defaultValue) {
            return new TypeParameterInfo(name, type, defaultValue, true);
        }


        public static TypeParameterInfo From(ParameterInfo param)
        {
            return new TypeParameterInfo(param.Name, new InstanceInfo(param.ParameterType), param.DefaultValue, param.HasDefaultValue);
        }
    }
}
