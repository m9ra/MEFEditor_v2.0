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
    /// <summary>
    /// Provider of testing environment settings.
    /// </summary>
    class SettingsProvider
    {
        /// <summary>
        /// The types that are loaded as direct.
        /// </summary>
        private static readonly Type[] directTypes = new Type[]{
            typeof(string),
            typeof(CILInstruction),
            typeof(VMStack),
            typeof(LiteralType)
        };

        /// <summary>
        /// The types that loaded with math support.
        /// </summary>
        private static readonly Type[] mathTypes = new Type[]{
            typeof(bool), typeof(int),typeof(double)
        };


        /// <summary>
        /// Initializes static members of the <see cref="SettingsProvider"/> class.
        /// </summary>
        static SettingsProvider()
        {
        }

        /// <summary>
        /// Creates the machine.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>Machine.</returns>
        internal static Machine CreateMachine(MachineSettings settings)
        {
            return new Machine(settings);
        }

        /// <summary>
        /// Creates the testing assembly.
        /// </summary>
        /// <returns>TestingAssembly.</returns>
        internal static TestingAssembly CreateTestingAssembly()
        {
            var settings = new MachineSettings(false);
            InitializeRuntime(settings.Runtime);

            var assembly = new TestingAssembly(settings);
            return assembly;
        }

        /// <summary>
        /// Initializes the Runtime.
        /// </summary>
        /// <param name="runtime">The Runtime.</param>
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

        /// <summary>
        /// Adds the direct type with math support.
        /// </summary>
        /// <param name="runtime">The runtime.</param>
        /// <param name="mathType">Type of the math.</param>
        public static void AddDirectMathType(RuntimeAssembly runtime, Type mathType)
        {
            var type = typeof(MathDirectType<>).MakeGenericType(mathType);

            var typeDefinition = Activator.CreateInstance(type) as DirectTypeDefinition;
            runtime.AddDirectDefinition(typeDefinition);
        }

        /// <summary>
        /// Adds the direct type.
        /// </summary>
        /// <param name="runtime">The runtime.</param>
        /// <param name="directType">Type of the direct.</param>
        public static void AddDirectType(RuntimeAssembly runtime, Type directType)
        {
            var typeDefinition = new DirectTypeDefinition(directType);
            runtime.AddDirectDefinition(typeDefinition);
        }

    }
}
