using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TypeSystem;
using TypeSystem.Runtime;

using Analyzing;
using Analyzing.Editing;
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

        public static TestingAssembly RunCIL(Func<object> sourceMethod)
        {
            var assembly = SettingsProvider.CreateTestingAssembly();

            assembly.AddMethod(Method.EntryMethodPath, sourceMethod.Method, Method.Entry_NoParam);

            addStandardMethods(assembly);

            return assembly;
        }

        public static TestingAssembly RunCIL(Action sourceMethod)
        {
            var assembly = SettingsProvider.CreateTestingAssembly();

            assembly.AddMethod(Method.EntryMethodPath, sourceMethod.Method, Method.Entry_NoParam);

            addStandardMethods(assembly);

            return assembly;
        }

        public static TestResult GetResult(this TestingAssembly assembly)
        {
            var entryLoader = new EntryPointLoader(
                Method.EntryInfo.MethodID
                , assembly.Loader);


            assembly.Runtime.BuildAssembly();

            var machine = SettingsProvider.CreateMachine(assembly.Settings);
            var entryObj = machine.CreateDirectInstance("EntryObject", new InstanceInfo(typeof(string)));
            var result = machine.Run(entryLoader, entryObj);

            foreach (var action in assembly.UserActions)
                action(result);

            var view = processEdits(result, assembly.EditActions);

            return new TestResult(view, result);
        }

        /// <summary>
        /// Test that entry source is equivalent to given source after edit actions
        /// </summary>
        /// <param name="source">Expected source</param>
        public static void AssertSourceEquivalence(this TestingAssembly assembly, string source)
        {
            var result = assembly.GetResult();
            var editedSource = assembly.GetEntrySource(result.View);

            var nSource = normalizeCode("{" + source + "}");
            var nEditedSource = normalizeCode(editedSource);

            Assert.AreEqual(nSource, nEditedSource);
        }

        public static string GetEntrySource(this TestingAssembly assembly, ExecutionView view)
        {
            return assembly.GetSource(Method.EntryInfo.MethodID, view);
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

        internal static TestCase AssertReturn(this TestingAssembly assembly)
        {
            return AssertVariable(assembly, null);
        }

        private static ExecutionView processEdits(AnalyzingResult result, IEnumerable<EditAction> editActions)
        {
            var view = result.CreateExecutionView();

            foreach (var editAction in editActions)
            {
                if (editAction.IsRemoveAction)
                {
                    view = processRemoveEdit(result, view, editAction);
                }
                else
                {
                    view = processInstanceEdit(result, view, editAction);
                }
            }
            view.Commit();
            return view;
        }

        private static ExecutionView processRemoveEdit(AnalyzingResult result, ExecutionView view, EditAction editAction)
        {
            var inst = result.EntryContext.GetValue(editAction.Variable);
            var success = view.Remove(inst);

            if (!success)
            {
                throw new NotSupportedException("Remove edit doesn't succeeded");
            }

            return view;
        }

        private static ExecutionView processInstanceEdit(AnalyzingResult result, ExecutionView view, EditAction editAction)
        {
            var inst = result.EntryContext.GetValue(editAction.Variable);
            foreach (var edit in inst.Edits)
            {
                if (edit.Name != editAction.Name)
                    continue;

                view.Apply(edit.Transformation);
                return view;
            }

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
