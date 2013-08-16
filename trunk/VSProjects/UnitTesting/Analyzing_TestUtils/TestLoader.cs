using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using TypeSystem;
using Analyzing;
using Analyzing.Execution;


namespace UnitTesting.Analyzing_TestUtils
{
    class TestLoaderProvider : TypeSystem.IInstructionLoader
    {
        EmitDirector _director;
        private TestLoaderProvider(EmitDirector director)
        {
            _director = director;
        }

        static internal TypeSystem.IInstructionLoader CreateStandardLoader(EmitDirector director)
        {
            var testLoader = new TestLoaderProvider(director);

            return new TypeSystem.DirectCallLoader(testLoader, Environment.SettingsProvider.TypeSettings);
        }

        public IInstructionGenerator<MethodID, InstanceInfo> EntryPoint
        {
            get { return new DirectorGenerator(_director); }
        }

        public VersionedName ResolveCallName(MethodID method, InstanceInfo[] staticArgumentInfo)
        {
                throw new NotImplementedException();
        }

        public IInstructionGenerator<MethodID, InstanceInfo> GetGenerator(VersionedName methodName)
        {
            return Environment.SettingsProvider.MethodGenerator(methodName);
        }
        
        public VersionedName ResolveStaticInitializer(TypeSystem.InstanceInfo info)
        {
            return new VersionedName(info.TypeName+"#initializer", -1);
        }
    }

    class DirectorGenerator:TypeSystem.IInstructionGenerator
    {
        readonly EmitDirector _director;
        public DirectorGenerator(EmitDirector director)
        {
            _director = director;
        }
        public void Generate(EmitterBase<MethodID, InstanceInfo> emitter)
        {
            _director(emitter);
        }
    }
}
