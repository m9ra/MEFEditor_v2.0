using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;

using MEFEditor.Analyzing;
using MEFEditor.Analyzing.Execution;

using MEFEditor.TypeSystem;
using MEFEditor.TypeSystem.Runtime;

using RecommendedExtensions.Core.Services;
using RecommendedExtensions.Core.Languages.CIL;
using RecommendedExtensions.Core.Languages.CSharp;

using MEFEditor.UnitTesting.RuntimeTypeDefinitions;
using MEFEditor.UnitTesting.TypeSystem_TestUtils;

namespace MEFEditor.UnitTesting.Analyzing_TestUtils.Environment
{
    class SettingsProvider
    {
        private static readonly Type[] directTypes = new Type[]{
            typeof(string),
            typeof(CILInstruction),
            typeof(VMStack),
            typeof(LiteralType)
        };

        private static readonly Type[] mathTypes = new Type[]{
            typeof(bool), typeof(int),typeof(double)
        };


        static SettingsProvider()
        {
        }

        internal static Machine CreateMachine(MachineSettings settings)
        {
            return new Machine(settings);
        }

        internal static TestingAssembly CreateTestingAssembly()
        {
            var settings = new MachineSettings(false);
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
            var type = typeof(MathDirectType<>).MakeGenericType(mathType);

            var typeDefinition = Activator.CreateInstance(type) as DirectTypeDefinition;
            runtime.AddDirectDefinition(typeDefinition);
        }

        public static void AddDirectType(RuntimeAssembly runtime, Type directType)
        {
            var typeDefinition = new DirectTypeDefinition(directType);
            runtime.AddDirectDefinition(typeDefinition);
        }

    }
}
