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
        readonly string _source;
        public ParsedGenerator(string source)
        {
            _source = source;
        }

        public void Generate(IEmitter<MethodID, InstanceInfo> emitter)
        {
            var codeNode = Parser.Parse(_source);
            var compiler = new Compiler(codeNode);

            compiler.GenerateInstructions(emitter);
        }
    }
}
