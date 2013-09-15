using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TypeSystem;
using TypeSystem.Runtime;

using Analyzing;
using Analyzing.Execution;

using UnitTesting.Analyzing_TestUtils;
using UnitTesting.Analyzing_TestUtils.Environment;

namespace UnitTesting.TypeSystem_TestUtils
{
    public static class AssemblyUtils
    {
        public static Instance EXTERNAL_INPUT { get; set; }
        public static Instance REPORTED_INSTANCE { get; internal set; }

    

        public static TestingAssembly Run(string entryMethodSource)
        {
            var assembly = SettingsProvider.CreateTestingAssembly();
            assembly.AddMethod(Method.EntryMethodPath, entryMethodSource, Method.Entry_NoParam);

            addStandardMethods(assembly);

            return assembly;
        }

        public static AnalyzingResult GetResult(this TestingAssembly assembly)
        {
            var entryLoader = new EntryPointLoader(
                Method.EntryInfo.MethodID
                , assembly.Loader);


            assembly.Runtime.BuildAssembly();

            var machine = SettingsProvider.CreateMachine();
            var entryObj = machine.CreateDirectInstance("EntryObject",new InstanceInfo(typeof(string)));
            var result = machine.Run(entryLoader, entryObj);

            foreach (var action in assembly.UserActions)
                action(result);

            processEdits(result, assembly.EditActions);

            return result;
        }

        /// <summary>
        /// Test that entry source is equivalent to given source after edit actions
        /// </summary>
        /// <param name="source">Expected source</param>
        public static void AssertSourceEquivalence(this TestingAssembly assembly, string source)
        {
            var result = assembly.GetResult();
            var editedSource = assembly.GetEntrySource();

            var nSource = normalizeCode("{" + source + "}");
            var nEditedSource = normalizeCode(editedSource);

            Assert.AreEqual(nSource, nEditedSource);
        }

        public static string GetEntrySource(this TestingAssembly assembly)
        {
            return assembly.GetSource(Method.EntryInfo.MethodID);
        }



        private static string normalizeCode(string code)
        {
            return code.Replace("\n", "").Replace("\r", "").Replace(" ", "");
        }

        internal static TestCase AssertVariable(this TestingAssembly assembly, string variableName)
        {
            var result = assembly.GetResult();

            return new TestCase(result, variableName);
        }

        private static void processEdits(AnalyzingResult result, IEnumerable<EditAction> editActions)
        {
            foreach (var editAction in editActions)
            {
                if (editAction.IsRemoveAction)
                {
                    processRemoveEdit(result, editAction);
                }
                else
                {
                    processInstanceEdit(result, editAction);
                }
            }
        }

        private static void processRemoveEdit(AnalyzingResult result, EditAction editAction)
        {
            var inst = result.EntryContext.GetValue(editAction.Variable);
            var services = result.CreateTransformationServices();

            var success = services.Remove(inst);

            if (!success)
            {
                throw new NotSupportedException("Remove edit doesn't succeeded");
            }

            services.Commit();
        }

        private static void processInstanceEdit(AnalyzingResult result, EditAction editAction)
        {
            var inst = result.EntryContext.GetValue(editAction.Variable);
            var edited = false;
            foreach (var edit in inst.Edits)
            {
                if (edit.Name != editAction.Name)
                    continue;

                var services = result.CreateTransformationServices();
                services.Apply(edit.Transformation);
                services.Commit();
                edited = true;
                break;
            }

            if (!edited)
                throw new KeyNotFoundException("Specified edit hasn't been found");
        }

        private static void addStandardMethods(TestingAssembly assembly)
        {
            assembly.AddMethod("Test.Report", (c) =>
            {
                AssemblyUtils.REPORTED_INSTANCE = c.CurrentArguments[1];
            }, Method.Void_StringParam);
        }

    }
}
