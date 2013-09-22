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
            typeof(bool),            
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
                AddDirectType(runtime, typeof(DirectTypeDefinition<>), directType);
            }

            foreach (var mathType in mathTypes)
            {
                AddDirectType(runtime, typeof(MathDirectType<>), mathType);
            }
        }

        public static void AddDirectType(RuntimeAssembly runtime, Type directDefinition, Type directType)
        {
            var isGeneric = directType.ContainsGenericParameters;
            if (isGeneric)
            {
                var genericArgs = new List<Type>();
                foreach (var param in directType.GetGenericArguments())
                {
                    genericArgs.Add(typeof(InstanceWrap));
                }

                directType = directType.MakeGenericType(genericArgs.ToArray());
            }

            var addDirectDefinition = runtime.GetType().GetMethod("AddDirectDefinition");
            var directTypeDef = directDefinition.MakeGenericType(directType);
            var runtimeType = Activator.CreateInstance(directTypeDef) as RuntimeTypeDefinition;
            runtimeType.IsGeneric = isGeneric;

            var genericAdd = addDirectDefinition.MakeGenericMethod(directType);
            genericAdd.Invoke(runtime, new object[] { runtimeType });

        }

        private static void onInstanceCreated(Instance instance)
        {
            _instances.Add(instance);
        }
    }
}
