using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;
using MEFEditor.TypeSystem;
using RecommendedExtensions.Core.Languages.CSharp;
using RecommendedExtensions.Core.Languages.CSharp.Compiling;
using RecommendedExtensions.Core.AssemblyProviders.ProjectAssembly;

namespace MEFEditor.UnitTesting.TypeSystem_TestUtils
{
    class ParsedGenerator : GeneratorBase, GenericMethodGenerator
    {
        static readonly SyntaxParser Parser = new SyntaxParser();

        private readonly TypeServices _services;

        private readonly IEnumerable<string> _genericParameters;

        public readonly TypeMethodInfo Method;

        public string SourceCode { get; internal set; }

        /// <summary>
        /// Source obtained from compiler after parsing is done
        /// </summary>
        public Source Source { get; private set; }


        public ParsedGenerator(TypeMethodInfo info, string sourceCode, IEnumerable<string> genericParameters, TypeServices services)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            if (sourceCode == null)
                throw new ArgumentNullException("sourceCode");

            if (genericParameters == null)
                throw new ArgumentNullException("genericParameters");

            if (services == null)
                throw new ArgumentNullException("services");

            SourceCode = sourceCode;
            Method = info;
            _services = services;
            _genericParameters = genericParameters;
        }

        protected override void generate(EmitterBase emitter)
        {
            var activation = new ParsingActivation(SourceCode, Method, _genericParameters);
            Source = Compiler.GenerateInstructions(activation, emitter, _services);
        }

        public ParsedGenerator ChangeSource(string newSource)
        {
            var newGenerator = new ParsedGenerator(Method, newSource, _genericParameters, _services);
            return newGenerator;
        }

        public MethodItem Make(PathInfo searchPath, TypeMethodInfo genericMethod)
        {
            var newMethod = genericMethod.MakeGenericMethod(searchPath);
            var newGenerator = new ParsedGenerator(newMethod, SourceCode, _genericParameters, _services);
            return new MethodItem(newGenerator, newMethod);
        }
    }
}
