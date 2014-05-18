using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Cecil;

using Analyzing;
using TypeSystem;

namespace AssemblyProviders.CILAssembly
{
    /// <summary>
    /// Generator used for argument initialization of composition points
    /// </summary>
    class ArgumentInitializerGenerator : GeneratorBase
    {
        /// <summary>
        /// Attribute which specify composition point arguments
        /// </summary>
        private readonly CustomAttribute _attribute;

        internal ArgumentInitializerGenerator(CustomAttribute attribute)
        {
            _attribute = attribute;
        }

        /// <inheritdoc />
        protected override void generate(EmitterBase emitter)
        {
            //first and only argument is parametric
            var resolvedArguments = resolveCustomArgument(_attribute.ConstructorArguments[0]) as object[];

            for (var i = 0; i < resolvedArguments.Length; ++i)
            {
                var argumentStorage = "arg" + i;
                var argumentValue = resolvedArguments[i];

                emitter.AssignLiteral(argumentStorage, argumentValue);
            }
        }

        /// <summary>
        /// Resolve objects from arguments of <see cref="CustomAttributeArgument"/> objects
        /// </summary>
        /// <param name="argumentObject">Object that can be present in <see cref="CustomAttributeArgument.Value"/></param>
        /// <returns>Resolved custom argument</returns>
        private object resolveCustomArgument(object argumentObject)
        {
            if (argumentObject is CustomAttributeArgument)
                return resolveCustomArgument(((CustomAttributeArgument)argumentObject).Value);

            var memberReference = argumentObject as MemberReference;
            if (memberReference != null)
            {
                var type = TypeDescriptor.Create(memberReference.FullName);
                return new CSharp.LiteralType(type);
            }

            var multiArgument = argumentObject as CustomAttributeArgument[];
            if (multiArgument != null)
            {
                var arguments = new List<object>();
                foreach (var singleArg in multiArgument)
                {
                    arguments.Add(resolveCustomArgument(singleArg));
                }

                return arguments.ToArray();
            }

            return argumentObject;
        }
    }
}
