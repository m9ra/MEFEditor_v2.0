using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using MEFEditor.Analyzing;
using MEFEditor.TypeSystem;
using RecommendedExtensions.Core.Languages.CIL;

namespace MEFEditor.UnitTesting.TypeSystem_TestUtils
{

    class CILGenerator : GeneratorBase, GenericMethodGenerator
    {
        public readonly TypeMethodInfo Info;

        public readonly CILMethod Source;

        private readonly TypeServices _services;

        public CILGenerator(TypeMethodInfo info, CILMethod source, TypeServices services)
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
            Compiler.GenerateInstructions(Source,Info,emitter,_services);
        }

        public MethodItem Make(PathInfo methodPath, TypeMethodInfo methodDefinition)
        {
            throw new NotImplementedException();
        }
    }
}
