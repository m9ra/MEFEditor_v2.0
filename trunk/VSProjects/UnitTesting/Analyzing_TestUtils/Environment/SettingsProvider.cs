using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;

using Analyzing;
using Analyzing.Execution;

using TypeSystem;
using TypeSystem.Runtime;

using AssemblyProviders.CIL;
using AssemblyProviders.TypeDefinitions;

using UnitTesting.RuntimeTypeDefinitions;
using UnitTesting.TypeSystem_TestUtils;

namespace UnitTesting.Analyzing_TestUtils.Environment
{
    class SettingsProvider
    {
        private static readonly Type[] directTypes = new Type[]{
            typeof(string),
            typeof(bool),   
            typeof(VMStack),
            typeof(LiteralType)
        };

        private static readonly Type[] mathTypes = new Type[]{
            typeof(int),typeof(double)
        };

        private static List<Instance> _instances = new List<Instance>();



        static SettingsProvider()
        {
        }

        internal static Machine CreateMachine(MachineSettings settings)
        {
            return new Machine(settings);
        }

        internal static TestingAssembly CreateTestingAssembly()
        {
            var settings = new MachineSettings(onInstanceCreated);
            InitializeRuntime(settings.Runtime);

            var assembly = new TestingAssembly(settings);
            return assembly;
        }

        internal static void InitializeRuntime(RuntimeAssembly runtime)
        {
            foreach (var directType in directTypes)
            {
                AddDirectType(runtime, directType);
            }

            foreach (var mathType in mathTypes)
            {
                AddDirectMathType(runtime, mathType);
            }
        }

        public static void AddDirectMathType(RuntimeAssembly runtime, Type mathType)
        {
            var type=typeof(MathDirectType<>).MakeGenericType(mathType);

            var typeDefinition = Activator.CreateInstance(type) as DirectTypeDefinition;
            runtime.AddDirectDefinition(typeDefinition);
        }

        public static void AddDirectType(RuntimeAssembly runtime, Type directType)
        {
            var typeDefinition = new DirectTypeDefinition(directType);
            runtime.AddDirectDefinition(typeDefinition);
        }

        private static void onInstanceCreated(Instance instance)
        {
            _instances.Add(instance);
        }
    }
}
