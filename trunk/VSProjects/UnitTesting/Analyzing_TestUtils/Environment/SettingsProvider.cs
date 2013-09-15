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

using UnitTesting.RuntimeTypeDefinitions;
using UnitTesting.TypeSystem_TestUtils;

namespace UnitTesting.Analyzing_TestUtils.Environment
{
    class SettingsProvider
    {
        private static readonly Type[] directTypes = new Type[]{
            typeof(string),
            typeof(bool)
        };

        private static readonly Type[] mathTypes = new Type[]{
            typeof(int),typeof(double)
        };

        private static List<Instance> _instances = new List<Instance>();

        static SettingsProvider()
        {
        }

        internal static Machine CreateMachine()
        {
            return new Machine(new MachineSettings(onInstanceCreated));
        }

        internal static TestingAssembly CreateTestingAssembly()
        {
            return new TestingAssembly(CreateRuntime());
        }

        internal static RuntimeAssembly CreateRuntime()
        {
            var runtime = new RuntimeAssembly();


            foreach (var directType in directTypes)
            {
                addType(runtime, typeof(DirectTypeDefinition<>), directType);
            }

            foreach (var mathType in mathTypes)
            {
                addType(runtime, typeof(MathDirectType<>), mathType);
            }

            return runtime;
        }

        private static void addType(RuntimeAssembly runtime, Type directDefinition, Type directType)
        {
            var addDirectDefinition = runtime.GetType().GetMethod("AddDirectDefinition");
            var directTypeDef = directDefinition.MakeGenericType(directType);
            var runtimeType = Activator.CreateInstance(directTypeDef);

            var genericAdd = addDirectDefinition.MakeGenericMethod(directType);
            genericAdd.Invoke(runtime, new object[] { runtimeType });
        }

        private static void onInstanceCreated(Instance instance)
        {
            _instances.Add(instance);
        }
    }
}
