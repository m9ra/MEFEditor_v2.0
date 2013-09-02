﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TypeSystem;
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

        public static readonly string EntryMethodName = "EntryMethod";

        public static TestingAssembly Run(string entryMethod)
        {
            var assembly = new TestingAssembly();
            assembly.AddMethod(EntryMethodName, entryMethod);

            addStandardMethods(assembly);

            return assembly;
        }

        public static AnalyzingResult<MethodID, InstanceInfo> GetResult(this TestingAssembly assembly)
        {
            var directAssembly = SettingsProvider.CreateDirectAssembly();
            var testAssemblies = new TestAssemblyCollection(assembly, directAssembly);
            var loader = new AssemblyLoader(testAssemblies);
            var entryLoader = new EntryPointLoader(
                new VersionedName(EntryMethodName, 0)
                , loader);


            var machine = new Machine<MethodID, InstanceInfo>(new MachineSettings());
            var entryObj = machine.CreateDirectInstance("EntryObject");
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
            return assembly.GetSource(EntryMethodName);
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

        private static void processEdits(AnalyzingResult<MethodID, InstanceInfo> result, IEnumerable<EditAction> editActions)
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

        private static void processRemoveEdit(AnalyzingResult<MethodID, InstanceInfo> result, EditAction editAction)
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

        private static void processInstanceEdit(AnalyzingResult<MethodID, InstanceInfo> result, EditAction editAction)
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
            assembly.AddMethod("Report", (c) =>
            {
                AssemblyUtils.REPORTED_INSTANCE = c.CurrentArguments[1];
            }, false, new ParameterInfo("p", new InstanceInfo("System.String")));
        }

    }
}
