using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mono.Cecil;

using MEFEditor.TypeSystem;
using MEFEditor.TypeSystem.Runtime;

using MEFEditor.Analyzing;
using MEFEditor.Analyzing.Editing;
using MEFEditor.Analyzing.Execution;

using MEFEditor.UnitTesting.Analyzing_TestUtils;
using MEFEditor.UnitTesting.Analyzing_TestUtils.Environment;

namespace MEFEditor.UnitTesting.TypeSystem_TestUtils
{
    /// <summary>
    /// Utility methods for providing assembly testing utilities.
    /// </summary>
    public static class AssemblyUtils
    {
        /// <summary>
        /// Simulate external input from user.
        /// </summary>
        /// <value>The external input.</value>
        public static Instance EXTERNAL_INPUT { get; set; }
        /// <summary>
        /// Instance which has been reported during analysis by Report call.
        /// </summary>
        /// <value>The reported instance.</value>
        public static Instance REPORTED_INSTANCE { get; internal set; }

        /// <summary>
        /// Runs the specified entry method source in C#.
        /// </summary>
        /// <param name="entryMethodSource">The entry method source.</param>
        /// <returns>TestingAssembly.</returns>
        public static TestingAssembly Run(string entryMethodSource)
        {
            return RunRaw("{" + entryMethodSource + "}");
        }

        /// <summary>
        /// Runs the specified entry method source in raw C# (no block parentheses are added).
        /// </summary>
        /// <param name="rawEntryMethodSource">The raw entry method source.</param>
        /// <returns>TestingAssembly.</returns>
        public static TestingAssembly RunRaw(string rawEntryMethodSource)
        {
            var assembly = SettingsProvider.CreateTestingAssembly();
            assembly.AddMethodRaw(Method.EntryMethodPath, rawEntryMethodSource, Method.Entry_NoParam);

            addStandardMethods(assembly);

            return assembly;
        }

        /// <summary>
        /// Finds method in assembly at given path with given method path.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <param name="methodPath">The method path.</param>
        /// <returns>MethodDefinition.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Cannot find method</exception>
        public static MethodDefinition FindMethod(string assemblyPath, string methodPath)
        {
            var pars = new ReaderParameters();
            var resolver = new DefaultAssemblyResolver();

            pars.AssemblyResolver = resolver;

            var cecilAssembly = AssemblyDefinition.ReadAssembly(assemblyPath, pars);

            //find type
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

        /// <summary>
        /// Runs the method from given assembly at given method path.
        /// CECIL transcription to IAL is used.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <param name="methodPath">The method path.</param>
        /// <returns>TestingAssembly.</returns>
        public static TestingAssembly RunCECIL(string assemblyPath, string methodPath)
        {
            var sourceMethod = FindMethod(assemblyPath, methodPath);
            var assembly = SettingsProvider.CreateTestingAssembly();

            var description = new MethodDescription(TypeDescriptor.Create(sourceMethod.ReturnType.FullName), false);
            assembly.AddMethod(Method.EntryMethodPath, sourceMethod, description);

            addStandardMethods(assembly);

            return assembly;
        }

        /// <summary>
        /// Runs the method from current assembly at given method path.
        /// CECIL transcription to IAL is used.
        /// </summary>
        /// <param name="methodPath">The method path.</param>
        /// <returns>TestingAssembly.</returns>
        public static TestingAssembly RunCurrentCECIL(string methodPath)
        {
            var assemblyFile = Assembly.GetEntryAssembly().Location;
            return RunCECIL(assemblyFile, methodPath);
        }

        /// <summary>
        /// Runs the given CIL method.
        /// </summary>
        /// <param name="sourceMethod">The CIL method.</param>
        /// <returns>TestingAssembly.</returns>
        public static TestingAssembly RunCIL(Func<object> sourceMethod)
        {
            var assembly = SettingsProvider.CreateTestingAssembly();

            var description = MethodDescription.Create<object>(false);
            assembly.AddMethod(Method.EntryMethodPath, sourceMethod.Method, description);

            addStandardMethods(assembly);

            return assembly;
        }

        /// <summary>
        /// Runs the given CIL method.
        /// </summary>
        /// <param name="sourceMethod">The source method.</param>
        /// <returns>TestingAssembly.</returns>
        public static TestingAssembly RunCIL(Action sourceMethod)
        {
            var assembly = SettingsProvider.CreateTestingAssembly();

            assembly.AddMethod(Method.EntryMethodPath, sourceMethod.Method, Method.Entry_NoParam);

            addStandardMethods(assembly);

            return assembly;
        }

        /// <summary>
        /// Gets the test result from test defined in assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="entryMethod">The entry method of test.</param>
        /// <returns>TestResult.</returns>
        public static TestResult GetResult(this TestingAssembly assembly, MethodID entryMethod)
        {
            if (!assembly.IsBuilded)
                assembly.Build();

            var entryObj = assembly.Machine.CreateInstance(TypeDescriptor.Create(Method.EntryClass));
            return GetResult(assembly, entryMethod, entryObj);
        }

        /// <summary>
        /// Gets the test result from test defined in assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="entryMethod">The entry method of test.</param>
        /// <param name="entryArguments">The entry method arguments.</param>
        /// <returns>TestResult.</returns>
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
        /// Test that entry source is equivalent to given source after edit actions.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="source">Expected source.</param>
        public static void AssertSourceEquivalence(this TestingAssembly assembly, string source)
        {
            var result = assembly.GetResult(Method.EntryInfo.MethodID);
            var editedSource = assembly.GetEntrySource(result.View);

            var nSource = normalizeCode("{" + source + "}");
            var nEditedSource = normalizeCode(editedSource);

            Assert.AreEqual(nSource, nEditedSource);
        }

        /// <summary>
        /// Gets the entry source.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="view">The view.</param>
        /// <returns>System.String.</returns>
        public static string GetEntrySource(this TestingAssembly assembly, ExecutionView view)
        {
            return assembly.GetSource(Method.EntryInfo.MethodID, view);
        }

        /// <summary>
        /// Normalizes the code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns>System.String.</returns>
        private static string normalizeCode(string code)
        {
            return code.Replace("\n", "").Replace("\r", "").Replace(" ", "");
        }

        /// <summary>
        /// Asserts condition on the variable. E.g. HasValue condition can be used.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="variableName">Name of the variable.</param>
        /// <returns>TestCase.</returns>
        internal static TestCase AssertVariable(this TestingAssembly assembly, string variableName)
        {
            var result = assembly.GetResult(Method.EntryInfo.MethodID);

            return new TestCase(result, variableName);
        }

        /// <summary>
        /// Asserts return value of entry method.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>TestCase.</returns>
        internal static TestCase AssertReturn(this TestingAssembly assembly)
        {
            return AssertVariable(assembly, null);
        }

        /// <summary>
        /// Processes given edits on results view.
        /// </summary>
        /// <param name="runtime">The runtime.</param>
        /// <param name="result">The result.</param>
        /// <param name="editActions">The edit actions.</param>
        /// <returns>ExecutionView.</returns>
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

        /// <summary>
        /// Processes given remove edits.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="view">The view.</param>
        /// <param name="editAction">The edit action.</param>
        /// <returns>ExecutionView.</returns>
        /// <exception cref="System.NotSupportedException">Remove edit doesn't succeeded</exception>
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

        /// <summary>
        /// Processes the instance edit.
        /// </summary>
        /// <param name="runtime">The runtime.</param>
        /// <param name="result">The result.</param>
        /// <param name="view">The view.</param>
        /// <param name="editAction">The edit action.</param>
        /// <returns>ExecutionView.</returns>
        /// <exception cref="System.NotSupportedException">Error occured during edit:  + lastError</exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Specified edit hasn't been found</exception>
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

        /// <summary>
        /// Adds the standard methods.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        private static void addStandardMethods(TestingAssembly assembly)
        {
            assembly.AddMethod("Test.Report", (c) =>
            {
                AssemblyUtils.REPORTED_INSTANCE = c.CurrentArguments[1];
            }, Method.Void_ObjectParam);
        }
    }
}
