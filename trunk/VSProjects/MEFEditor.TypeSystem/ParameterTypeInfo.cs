using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using MEFEditor.Analyzing;

namespace MEFEditor.TypeSystem
{
    /// <summary>
    /// Type system information for method parameter.
    /// </summary>
    public class ParameterTypeInfo
    {
        /// <summary>
        /// The name of parameter.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The typeof parameter.
        /// </summary>
        public readonly TypeDescriptor Type;

        /// <summary>
        /// The default parameter value.
        /// </summary>
        public readonly object DefaultValue;

        /// <summary>
        /// The has default value indicator.
        /// </summary>
        public readonly bool HasDefaultValue;

        /// <summary>
        /// The has variable count of arguments indicator.
        /// </summary>
        public readonly bool HasParam;

        /// <summary>
        /// Prepared empty array of parameters for methods without any parameters.
        /// </summary>
        public static readonly ParameterTypeInfo[] NoParams = new ParameterTypeInfo[0];

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypeInfo" /> class.
        /// </summary>
        /// <param name="name">The name of parameter.</param>
        /// <param name="type">The type of parameter.</param>
        /// <param name="defaultValue">The default parameter value.</param>
        /// <param name="hasDefaultValue">if set to <c>true</c> parameter will have default value.</param>
        /// <param name="hasParam">if set to <c>true</c> parameter will have variable count of arguments.</param>
        private ParameterTypeInfo(string name, TypeDescriptor type, object defaultValue, bool hasDefaultValue, bool hasParam)
        {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
            HasDefaultValue = hasDefaultValue;
            HasParam = hasParam;
        }

        /// <summary>
        /// Factory method for parameter with specified name and type. 
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <returns>ParameterTypeInfo.</returns>
        public static ParameterTypeInfo Create(string name, TypeDescriptor type)
        {
            return new ParameterTypeInfo(name, type, null, false, false);
        }

        /// <summary>
        /// Makes generic specialization of parameter.
        /// </summary>
        /// <param name="substitutedType">Substituted type.</param>
        /// <returns>ParameterTypeInfo.</returns>
        public ParameterTypeInfo MakeGeneric(TypeDescriptor substitutedType)
        {
            return new ParameterTypeInfo(Name, substitutedType, DefaultValue, HasDefaultValue, HasParam);
        }

        /// <summary>
        /// Factory method for parameter created from given <see cref="System.Reflection"/> representation. 
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <param name="paramType">Type of the parameter.</param>
        /// <returns>ParameterTypeInfo.</returns>
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
            var hasDefault = param.DefaultValue != System.DBNull.Value;
            return new ParameterTypeInfo(name, paramType, param.DefaultValue, hasDefault, hasParam);
        }
    }
}
