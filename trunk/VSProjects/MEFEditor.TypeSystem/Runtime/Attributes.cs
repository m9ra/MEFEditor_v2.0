using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;

namespace MEFEditor.TypeSystem.Runtime
{
    /// <summary>
    /// Attribute used for decorating runtime type definition methods to
    /// explicit typing its arguments.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ParameterTypesAttribute : Attribute
    {
        /// <summary>
        /// The parameter types
        /// </summary>
        public readonly IEnumerable<TypeDescriptor> ParameterTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypesAttribute"/> class.
        /// </summary>
        /// <param name="parameterTypes">The method definition parameter types.</param>
        public ParameterTypesAttribute(params Type[] parameterTypes)
        {
            ParameterTypes = from parameterType in parameterTypes select TypeDescriptor.Create(parameterType);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypesAttribute"/> class.
        /// </summary>
        /// <param name="parameterTypes">The method definition parameter types.</param>
        public ParameterTypesAttribute(params string[] parameterTypes)
        {
            ParameterTypes = from parameterType in parameterTypes select TypeDescriptor.Create(parameterType);
        }
    }

    /// <summary>
    /// Attribute used for decorating runtime type definition methods to
    /// explicit typing its return values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ReturnTypeAttribute : Attribute
    {
        /// <summary>
        /// The return type descriptor.
        /// </summary>
        public readonly TypeDescriptor ReturnInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReturnTypeAttribute"/> class.
        /// </summary>
        /// <param name="returnType">Type of the return value.</param>
        public ReturnTypeAttribute(Type returnType)
        {
            ReturnInfo = TypeDescriptor.Create(returnType);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReturnTypeAttribute"/> class.
        /// </summary>
        /// <param name="returnTypeFullname">Fullname of return type.</param>
        public ReturnTypeAttribute(string returnTypeFullname)
        {
            ReturnInfo = TypeDescriptor.Create(returnTypeFullname);
        }
    }
}
