using System;
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

        /// <summary>
        /// Simulate actions from user
        /// </summary>
        public IEnumerable<ResultAction> UserActions { get { return _userActions; } }
        public IEnumerable<EditAction> EditActions { get { return _editActions; } }



        public TestingAssembly(RuntimeAssembly runtime)
        {
            Assemblies = new TestAssemblyCollection(runtime,this);
            Runtime = runtime;

            Loader = new AssemblyLoader(Assemblies);
        }

        public TestingAssembly AddMethod(string path, string code, bool isStatic = false,string returnType="System.Void", params ParameterInfo[] parameters)
        {
            var info = getInfo(path, isStatic,returnType, parameters);
            var source = new Source("{" + code + "}");
            var method = new ParsedGenerator(info, source,TypeServices);


            addMethod(method, info);
            return this;
        }

        public TestingAssembly AddMethod(string path, DirectMethod source, bool isStatic = false, params ParameterInfo[] parameters)
        {
            var info = getInfo(path, isStatic,"System.Void", parameters);
            var method = new DirectGenerator(source);

            addMethod(method, info);
            return this;
        }

        public TestingAssembly AddMethod(string path, DirectMethod source, string returnType, params ParameterInfo[] parameters)
        {
            var info = getInfo(path, false, returnType, parameters);
            var method = new DirectGenerator(source);

            addMethod(method, info);
            return this;
        }

        public TestingAssembly AddToRuntime<T>()
            where T:DataTypeDefinition
        {
            var runtimeTypeDef= Activator.CreateInstance<T>();
            Runtime.AddDefinition(runtimeTypeDef);

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
            var parsedGenerator=_methods.AccordingId(method) as ParsedGenerator;
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

        #endregion

        #region Private utils
        private void addMethod(GeneratorBase method, TypeMethodInfo info)
        {
            _methods.AddItem(new MethodItem(method, info));
        }

        private TypeMethodInfo getInfo(string path, bool isStatic, string returnType, params ParameterInfo[] parameters)
        {
            var nameParts = path.Split('.');
            var methodName = nameParts.Last();
            var typeName = string.Join(".", nameParts.Take(nameParts.Count() - 1).ToArray());

            var typeInfo = new InstanceInfo(typeName);
            var returnInfo = new InstanceInfo(returnType);

            return new TypeMethodInfo(typeInfo, methodName, returnInfo, parameters, isStatic);
        }

        #endregion

    }
}
