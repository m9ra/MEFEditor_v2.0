using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using Analyzing.Execution;


namespace UnitTesting.Analyzing_TestUtils
{
    class TestLoader : IInstructionLoader, IInstructionGenerator
    {
        EmitDirector _director;
        public TestLoader(EmitDirector director)
        {
            _director = director;
        }

        public IInstructionGenerator EntryPoint
        {
            get { return this; }
        }

        public VersionedName Name
        {
            get { return new VersionedName("TestGenerator", 1); }
        }

        public void Generate(IEmitter emitter)
        {
            _director(emitter);
        }

        public TypeDescription ResolveDescription(string typeFullname)
        {
            return new TypeDescription(typeFullname);
        }

        public VersionedName ResolveCallName(MethodDescription description)
        {
            return new VersionedName(description.ThisType.Fullname + "." + description.MethodName,0);
        }

        public IInstructionGenerator GetGenerator(VersionedName methodName)
        {
            return new TestLoader((e) =>
            {
                e.AssignLiteral("result", 20);
                e.Return("result");
            });
        }
    }
}
