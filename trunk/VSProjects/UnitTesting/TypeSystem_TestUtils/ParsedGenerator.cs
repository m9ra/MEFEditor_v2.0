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

        public ParsedGenerator(string source)
        {
            _methodSource = source;
        }

        public void Generate(IEmitter<MethodID, InstanceInfo> emitter)
        {
            var method = Parser.Parse(_methodSource);
            Compiler.GenerateInstructions(method,emitter);
        }
    }
}
