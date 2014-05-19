using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Cecil;

using TypeSystem;

namespace AssemblyProviders.CIL
{
    class TranscriptionContext
    {
        public readonly TypeReferenceHelper TypeHelper=new TypeReferenceHelper();

        public TranscriptionContext(TypeMethodInfo methodInfo, IEnumerable<GenericParameter> genericParameters)
        {
            var arguments = methodInfo.Path.GenericArgs.ToArray();
            var parameters = genericParameters.ToArray();

            for (var i = 0; i < parameters.Length; ++i)
            {
                var parameterName=parameters[i];
                var argument=TypeDescriptor.Create(arguments[i]);
                TypeHelper.Substitutions[parameterName] = argument;
            }
        }
    }
}
