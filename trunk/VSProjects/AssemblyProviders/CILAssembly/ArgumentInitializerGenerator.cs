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
            var resolvedArguments = CILAssembly.ResolveCustomAttributeArgument(_attribute.ConstructorArguments[0]) as object[];

            for (var i = 0; i < resolvedArguments.Length; ++i)
            {
                var argumentStorage = "arg" + i;
                var argumentValue = resolvedArguments[i];

                emitter.AssignLiteral(argumentStorage, argumentValue);
            }
        }      
    }
}
