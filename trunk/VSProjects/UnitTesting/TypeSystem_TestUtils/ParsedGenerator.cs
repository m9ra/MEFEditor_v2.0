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

        readonly string _methodSource;

        TypeServices _services;

        public ParsedGenerator(TypeMethodInfo info,string source)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            
            if (source == null)
                throw new ArgumentNullException("source");

            

            _methodSource = source;
            Info = info;
        }

        internal void SetServices(TypeServices services)
        {
            _services = services;
        }

        protected override void generate(EmitterBase<MethodID, InstanceInfo> emitter)
        {
            var method = Parser.Parse(_methodSource);
            Compiler.GenerateInstructions(method,Info,emitter,_services);
        }
    }
}
