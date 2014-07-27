using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EnvDTE;

using MEFEditor.Analyzing;
using MEFEditor.TypeSystem;

namespace RecommendedExtensions.Core.AssemblyProviders.ProjectAssembly.MethodBuilding
{
    class InitializerGenerator : GeneratorBase
    {
        private readonly TypeMethodInfo _compositionPoint;
        private readonly AttributeInfo _compositionAttribute;
        private readonly VsProjectAssembly _declaringAssembly;

        internal InitializerGenerator(VsProjectAssembly declaringAssembly, AttributeInfo compositionAttribute, TypeMethodInfo compositionPoint)
        {
            _declaringAssembly = declaringAssembly;
            _compositionPoint = compositionPoint;
            _compositionAttribute = compositionAttribute;
        }

        protected override void generate(EmitterBase emitter)
        {
            var namespaces = _declaringAssembly.GetNamespaces(_compositionAttribute.Element as CodeElement);


            var initializerMethodInfo = new TypeMethodInfo(_compositionPoint.DeclaringType, ParsingActivation.InlineMethodName,
                          TypeDescriptor.Void, ParameterTypeInfo.NoParams, false, TypeDescriptor.NoDescriptors);

            var code = new StringBuilder();
            code.AppendLine("{");
            for (var i = 0; i < _compositionPoint.Parameters.Length; ++i)
            {
                var parameter = _compositionPoint.Parameters[i];
                var argument = _compositionAttribute.GetArgument(i);

                if (argument == null)
                    argument = "null";

                code.AppendLine(" var arg" + i + " = " + argument + ";");
            }
            code.AppendLine("}");

            var activation = new ParsingActivation(code.ToString(), initializerMethodInfo, new string[0], namespaces);

            _declaringAssembly.ParsingProvider(activation, emitter);
        }
    }
}
