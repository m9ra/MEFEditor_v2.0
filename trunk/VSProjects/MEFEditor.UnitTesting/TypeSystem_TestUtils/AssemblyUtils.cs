using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mono.Cecil;

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
            return RunRaw("{" + entryMethodSource + "}");
        }

        public static TestingAssembly RunRaw(string rawEntryMethodSource)
        {
            var assembly = SettingsProvider.CreateTestingAssembly();
            assembly.AddMethodRaw(Method.EntryMethodPath, rawEntryMethodSource, Method.Entry_NoParam);

            addStandardMethods(assembly);

            return assembly;
        }

        public static MethodDefinition FindMethod(string assemblyPath, string methodPath)
        {
            var pars = new ReaderParameters();
            var resolver = new DefaultAssemblyResolver();

            pars.AssemblyResolver = resolver;

            var cecilAssembly = AssemblyDefinition.ReadAssembly(assemblyPath, pars);

            /*foreach (var rf in cecilAssembly.MainModule.AssemblyReferences)
            {
                string path;
                var refAssembly = cecilAssembly.MainModule.AssemblyResolver.Resolve(rf);
                path = refAssembly.MainModule.FullyQualifiedName;
            }*/


            foreach (var type in cecilAssembly.MainModule.Types)
            {
                if (!methodPath.StartsWith(type.FullName))
                    continue;

                var name = Naming.SplitGenericPath(methodPath).Last();

                foreach (var method in type.Methods)
                {
                    if (name != method.Name)
                        continue;

                    return method;
                }
            }

            throw new KeyNotFoundException("Cannot find method: " + methodPath);
        }

        public static TestingAssembly RunCECIL(string assemblyPath, string methodPath)
        {
            var sourceMethod = FindMethod(assemblyPath, methodPath);
            var assembly = SettingsProvider.CreateTestingAssembly();

            var description = new MethodDescription(TypeDescriptor.Create(sourceMethod.ReturnType.FullName), false);
            assembly.AddMethod(Method.EntryMethodPath, sourceMethod, description);

            addStandardMethods(assembly);

            return assembly;
        }

        public static TestingAssembly RunCurrentCECIL(string methodPath)
        {
            var assemblyFile = Assembly.GetEntryAssembly().Location;
            return RunCECIL(assemblyFile, methodPath);
        }

        public static TestingAssembly RunCIL(Func<object> sourceMethod)
        {
            var assembly = SettingsProvider.CreateTestingAssembly();

            var description = MethodDescription.Create<object>(false);
            assembly.AddMethod(Method.EntryMethodPath, sourceMethod.Method, description);

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

        public static TestResult GetResult(this TestingAssembly assembly, MethodID entryMethod)
        {
            if (!assembly.IsBuilded)
                assembly.Build();

            var entryObj = assembly.Machine.CreateInstance(TypeDescriptor.Create(Method.EntryClass));
            return GetResult(assembly, entryMethod, entryObj);
        }

        public static TestResult GetResult(this TestingAssembly assembly, MethodID entryMethod, params Instance[] entryArguments)
        {
            if (!assembly.IsBuilded)
                assembly.Build();

            var result = assembly.Machine.Run(assembly.Loader, entryMethod, entryArguments);

            foreach (var action in assembly.UserActions)
                action(result);

            var view = processEdits(assembly.Runtime, result, assembly.EditActions);

            return new TestResult(view, result);
        }


        /// <summary>
        /// Test that entry source is equivalent to given source after edit actions
        /// </summary>
        /// <param name="source">Expected source</param>
        public static void AssertSourceEquivalence(this TestingAssembly assembly, string source)
        {
            var result = assembly.GetResult(Method.EntryInfo.MethodID);
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
            var result = assembly.GetResult(Method.EntryInfo.MethodID);

            return new TestCase(result, variableName);
        }

        internal static TestCase AssertReturn(this TestingAssembly assembly)
        {
            return AssertVariable(assembly, null);
        }

        private static ExecutionView processEdits(RuntimeAssembly runtime, AnalyzingResult result, IEnumerable<EditAction> editActions)
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
                    view = processInstanceEdit(runtime, result, view, editAction);
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

        private static ExecutionView processInstanceEdit(RuntimeAssembly runtime, AnalyzingResult result, ExecutionView view, EditAction editAction)
        {
            var editOwner = result.EntryContext.GetValue(editAction.Variable);
            string lastError = null;
            foreach (var edit in editOwner.Edits)
            {
                if (edit.Name != editAction.Name)
                    continue;

                var editView = new EditView(view);
                var resultView = runtime.RunEdit(edit, editView);
                editView = (resultView as EditView);

                if (editView.HasError)
                {
                    lastError = editView.Error;
                    continue;
                }

                return editView.CopyView();
            }

            if (lastError != null)
                throw new NotSupportedException("Error occured during edit: " + lastError);

            throw new KeyNotFoundException("Specified edit hasn't been found");
        }

        private static void addStandardMethods(TestingAssembly assembly)
        {
            assembly.AddMethod("Test.Report", (c) =>
            {
                AssemblyUtils.REPORTED_INSTANCE = c.CurrentArguments[1];
            }, Method.Void_ObjectParam);
        }

    }
}
