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
    class ParsedGenerator:GeneratorBase
    {
        static readonly SyntaxParser Parser = new SyntaxParser();

        public readonly TypeMethodInfo Info;

        public readonly Source Source;

        TypeServices _services;

        public ParsedGenerator(TypeMethodInfo info,Source source)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            
            if (source == null)
                throw new ArgumentNullException("source");

            

            Source = source;
            Info = info;
        }

        internal void SetServices(TypeServices services)
        {
            _services = services;
        }

        protected override void generate(EmitterBase emitter)
        {
            var method = Parser.Parse(Source);
            Compiler.GenerateInstructions(method,Info,emitter,_services);
        }
    }
}
