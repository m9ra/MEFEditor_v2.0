using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssemblyProviders.CSharp.Compiling;
using AssemblyProviders.CSharp;
using TypeSystem;
using Analyzing;

namespace UnitTesting.TypeSystem_TestUtils
{
    interface GenericMethodGenerator
    {
        GenericMethodProvider GetProvider();
    }

    class ParsedGenerator : GeneratorBase, GenericMethodGenerator
    {
        static readonly SyntaxParser Parser = new SyntaxParser();

        public readonly TypeMethodInfo Info;

        public readonly Source Source;

        private readonly TypeServices _services;

        public ParsedGenerator(TypeMethodInfo info, Source source, TypeServices services)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            if (source == null)
                throw new ArgumentNullException("source");

            Source = source;
            Info = info;
            _services = services;
        }


        protected override void generate(EmitterBase emitter)
        {
            var method = Parser.Parse(Source);
            Compiler.GenerateInstructions(method, Info, emitter, _services);
        }

        public GenericMethodProvider GetProvider()
        {
            return genericMethodProvider;
        }

        private MethodItem genericMethodProvider(PathInfo searchPath, TypeMethodInfo genericMethod)
        {
            var newMethod = genericMethod.MakeGenericMethod(searchPath);            
            var newGenerator = new ParsedGenerator(newMethod, Source, _services);
            return new MethodItem(newGenerator, newMethod);
        }
    }
}
