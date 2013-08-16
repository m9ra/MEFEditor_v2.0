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
    class ParsedGenerator:IInstructionGenerator
    {
        static readonly SyntaxParser Parser = new SyntaxParser();

        readonly string _methodSource;

        TypeServices _services;

        ParameterInfo[] _arguments;

        public ParsedGenerator(string source,ParameterInfo[] arguments)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (arguments == null)
                throw new ArgumentNullException("arguments");

            _methodSource = source;
            _arguments = arguments;
        }

        internal void SetServices(TypeServices services)
        {
            _services = services;
        }

        public void Generate(EmitterBase<MethodID, InstanceInfo> emitter)
        {
            var method = Parser.Parse(_methodSource);
            Compiler.GenerateInstructions(method,_arguments,emitter,_services);
        }
    }
}
