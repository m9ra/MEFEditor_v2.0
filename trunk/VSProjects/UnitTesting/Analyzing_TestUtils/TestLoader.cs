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
    class TestLoaderProvider : LoaderBase
    {
        DirectAssembly _assembly = new DirectAssembly(Environment.SettingsProvider.TypeSettings);
        EmitDirector _director;
        private TestLoaderProvider(EmitDirector director)
        {
            _director = director;
        }

        static internal LoaderBase CreateStandardLoader(EmitDirector director)
        {
            var testLoader = new TestLoaderProvider(director);

            return testLoader;
        }

        public override GeneratorBase EntryPoint
        {
            get { return new DirectorGenerator(_director); }
        }

        public override VersionedName ResolveCallName(MethodID method, InstanceInfo[] staticArgumentInfo)
        {
            //TODO better method resolving

            var methodName = staticArgumentInfo[0].TypeName + "." + method.MethodName;
            if (!_assembly.DirectMethods.ContainsKey(methodName))
            {
                methodName = Name.From(method, staticArgumentInfo).Name;
                methodName = staticArgumentInfo[0].TypeName + "." + methodName;
            }

            var name = new VersionedName(methodName, 733);
            return name;
        }

        public override GeneratorBase GetGenerator(VersionedName methodName)
        {
            MethodItem method;

            if (_assembly.DirectMethods.TryGetValue(methodName.Name, out method))
            {
                return method.Generator;
            }

            return Environment.SettingsProvider.MethodGenerator(methodName);
        }
                
        public override VersionedName ResolveStaticInitializer(InstanceInfo info)
        {
            return new VersionedName(info.TypeName+"#initializer", -1);
        }
    }

    class DirectorGenerator:GeneratorBase
    {
        readonly EmitDirector _director;
        public DirectorGenerator(EmitDirector director)
        {
            _director = director;
        }
        protected override void generate(EmitterBase emitter)
        {
            _director(emitter);
        }
    }
}
