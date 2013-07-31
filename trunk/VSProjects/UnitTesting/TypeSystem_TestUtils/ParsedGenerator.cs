using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AssemblyProviders.CSharp;
using TypeSystem;
using Analyzing;

namespace UnitTesting.TypeSystem_TestUtils
{
    class ParsedGenerator:IInstructionGenerator
    {
        static readonly SyntaxParser Parser = new SyntaxParser();

        readonly string _methodSource;

        TypeServices _services;

        public ParsedGenerator(string source)
        {
            if (source == null)
                throw new ArgumentNullException("source");


            _methodSource = source;
        }

        internal void SetServices(TypeServices services)
        {
            _services = services;
        }

        public void Generate(IEmitter<MethodID, InstanceInfo> emitter)
        {
            var method = Parser.Parse(_methodSource);
            Compiler.GenerateInstructions(method,emitter,_services);
        }
    }
}
