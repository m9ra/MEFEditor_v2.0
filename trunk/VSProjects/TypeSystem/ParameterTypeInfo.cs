﻿using System;
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
        public readonly TypeDescriptor Type;
        public readonly object DefaultValue;
        public readonly bool HasDefaultValue;
        public readonly bool HasParam;

        public static readonly ParameterTypeInfo[] NoParams = new ParameterTypeInfo[0];

        private ParameterTypeInfo(string name, TypeDescriptor type, object defaultValue, bool hasDefaultValue, bool hasParam)
        {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
            HasDefaultValue = hasDefaultValue;
            HasParam = hasParam;
        }

        public static ParameterTypeInfo Create(string name, TypeDescriptor type)
        {
            return new ParameterTypeInfo(name, type, null, false, false);
        }

        public ParameterTypeInfo MakeGeneric(TypeDescriptor substitutedType)
        {
            return new ParameterTypeInfo(Name, substitutedType, DefaultValue, HasDefaultValue, HasParam);
        }

        public static ParameterTypeInfo From(ParameterInfo param, TypeDescriptor paramType = null)
        {
            var name = param.Name;
            if (name == null)
            {
                //default name
                name = "p" + param.Position;
            }

            if (paramType == null)
            {
                paramType = TypeDescriptor.Create(param.ParameterType);
            }

            var hasParam = param.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0;
            return new ParameterTypeInfo(name, paramType, param.DefaultValue, param.HasDefaultValue, hasParam);
        }
    }
}
