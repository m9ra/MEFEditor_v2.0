using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using Analyzing;

namespace TypeSystem
{
    public class ParameterTypeInfo
    {
        public readonly string Name;
        public readonly InstanceInfo Type;
        public readonly object DefaultValue;
        public readonly bool HasDefaultValue;

        private ParameterTypeInfo(string name, InstanceInfo type, object defaultValue, bool hasDefaultValue)
        {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
            HasDefaultValue = hasDefaultValue;
        }

        public static ParameterTypeInfo Create(string name, InstanceInfo type)
        {
            return new ParameterTypeInfo(name, type, null, false);
        }

        public static ParameterTypeInfo CreateWithDefault(string name, InstanceInfo type, object defaultValue) {
            return new ParameterTypeInfo(name, type, defaultValue, true);
        }


        public static ParameterTypeInfo From(ParameterInfo param)
        {
            return new ParameterTypeInfo(param.Name, new InstanceInfo(param.ParameterType), param.DefaultValue, param.HasDefaultValue);
        }
    }
}
