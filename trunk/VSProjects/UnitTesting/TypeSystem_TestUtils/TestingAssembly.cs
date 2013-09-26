﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using TypeSystem;
using TypeSystem.Runtime;

using AssemblyProviders.CSharp;
using AssemblyProviders.CSharp.Compiling;

using UnitTesting.Analyzing_TestUtils;
using UnitTesting.Analyzing_TestUtils.Environment;

namespace UnitTesting.TypeSystem_TestUtils
{
    public delegate void ResultAction(AnalyzingResult result);

    public class TestingAssembly : AssemblyProvider
    {
        HashedMethodContainer _methods = new HashedMethodContainer();
        List<EditAction> _editActions = new List<EditAction>();
        List<ResultAction> _userActions = new List<ResultAction>();

        internal readonly TestAssemblyCollection Assemblies;

        internal readonly AssemblyLoader Loader;

        /// <summary>
        /// because of accessing runtime adding services for testing purposes
        /// </summary>
        public readonly RuntimeAssembly Runtime;

        public readonly MachineSettings Settings;

        /// <summary>
        /// Simulate actions from user
        /// </summary>
        public IEnumerable<ResultAction> UserActions { get { return _userActions; } }
        public IEnumerable<EditAction> EditActions { get { return _editActions; } }



        public TestingAssembly(MachineSettings settings)
        {
            Settings = settings;
            Runtime = settings.Runtime;
            Assemblies = new TestAssemblyCollection(Runtime, this);

            Loader = new AssemblyLoader(Assemblies);
        }

        public TestingAssembly AddMethod(string methodPath, string code, MethodDescription description)
        {
            var methodInfo = buildDescription(description, methodPath);

            var source = new Source("{" + code + "}", methodInfo);
            var method = new ParsedGenerator(methodInfo, source, TypeServices);
            addMethod(method, methodInfo, description.Implemented);

            return this;
        }

        public TestingAssembly AddMethod(string methodPath, DirectMethod source, MethodDescription description)
        {

            var methodInfo = buildDescription(description, methodPath);

            var method = new DirectGenerator(source);
            addMethod(method, methodInfo, description.Implemented);

            return this;
        }

        public TestingAssembly AddToRuntime<T>()
            where T : DataTypeDefinition
        {
            var runtimeTypeDef = Activator.CreateInstance<T>();
            Runtime.AddDefinition(runtimeTypeDef);

            return this;
        }

        public TestingAssembly AddDirectToRuntime<T>()
        {
            SettingsProvider.AddDirectType(Runtime, typeof(DirectTypeDefinition<>), typeof(T));
            return this;
        }

        /// <summary>
        /// Generic parameters has to be satisfiable by Instance
        /// </summary>
        /// <param name="genericType">Type which generic arguments will be substituted by WrappedInstance</param>
        /// <returns></returns>
        public TestingAssembly AddWrappedGenericToRuntime(Type genericType)
        {

            SettingsProvider.AddDirectType(Runtime, typeof(DirectTypeDefinition<>), genericType);
            return this;
        }

        public TestingAssembly UserAction(ResultAction action)
        {
            _userActions.Add(action);

            return this;
        }

        public TestingAssembly AddEditAction(string variable, string editName)
        {
            var editAction = EditAction.Edit(new VariableName(variable), editName);
            _editActions.Add(editAction);
            return this;
        }

        public TestingAssembly AddRemoveAction(string variable)
        {
            var editAction = EditAction.Remove(new VariableName(variable));
            _editActions.Add(editAction);
            return this;
        }

        public string GetSource(MethodID method)
        {
            var parsedGenerator = _methods.AccordingId(method) as ParsedGenerator;
            return parsedGenerator.Source.Code;
        }

        #region Assembly provider implementatation

        public override SearchIterator CreateRootIterator()
        {
            return new HashIterator(_methods);
        }

        public override GeneratorBase GetMethodGenerator(MethodID method)
        {
            return _methods.AccordingId(method);
        }

        public override GeneratorBase GetGenericMethodGenerator(MethodID method, PathInfo searchPath)
        {
            return _methods.AccordingGenericId(method, searchPath);
        }

        public override MethodID GetImplementation(MethodID method, InstanceInfo dynamicInfo)
        {
            return _methods.GetImplementation(method, dynamicInfo);
        }

        public override MethodID GetGenericImplementation(MethodID methodID, PathInfo methodSearchPath, PathInfo implementingTypePath)
        {
            return _methods.GetGenericImplementation(methodID, methodSearchPath, implementingTypePath);
        }

        public override InheritanceChain GetInheritanceChain(PathInfo typePath)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private utils

        private TypeMethodInfo buildDescription(MethodDescription description, string methodPath)
        {
            var info = description.CreateInfo(methodPath);
            return info;
        }

        private void addMethod(GeneratorBase method, TypeMethodInfo info, IEnumerable<InstanceInfo> implementedTypes)
        {
            var implemented = implementedTypes.ToArray();
            if (info.HasGenericParameters)
            {
                var genericMethod = method as GenericMethodGenerator;
                _methods.AddItem(new MethodItem(genericMethod.GetProvider(), info), implemented);
            }
            else
            {
                _methods.AddItem(new MethodItem(method, info), implemented);
            }
        }

        #endregion
    }
}
