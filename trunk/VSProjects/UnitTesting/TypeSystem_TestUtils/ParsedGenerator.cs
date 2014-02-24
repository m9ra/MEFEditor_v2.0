using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using TypeSystem;
using AssemblyProviders.CSharp;
using AssemblyProviders.CSharp.Compiling;

namespace UnitTesting.TypeSystem_TestUtils
{
    class ParsedGenerator : GeneratorBase, GenericMethodGenerator
    {
        static readonly SyntaxParser Parser = new SyntaxParser();

        public readonly TypeMethodInfo Info;

        public readonly TypeMethodInfo InfoDefinition;

        public Source Source { get; internal set; }

        private readonly TypeServices _services;

        public ParsedGenerator(TypeMethodInfo info, Source source, TypeServices services)
            : this(info, source, services, info)
        { }

        private ParsedGenerator(TypeMethodInfo info, Source source, TypeServices services, TypeMethodInfo infoDefinition)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            if (source == null)
                throw new ArgumentNullException("source");

            Source = source;
            Info = info;
            InfoDefinition = infoDefinition;
            _services = services;
        }

        protected override void generate(EmitterBase emitter)
        {
            var method = Parser.Parse(Source);
            Compiler.GenerateInstructions(method, Info, InfoDefinition, emitter, _services);
        }

        public MethodItem Make(PathInfo searchPath, TypeMethodInfo genericMethod)
        {
            var newMethod = genericMethod.MakeGenericMethod(searchPath);
            var newGenerator = new ParsedGenerator(newMethod, Source, _services, InfoDefinition);
            return new MethodItem(newGenerator, newMethod);
        }
    }
}
