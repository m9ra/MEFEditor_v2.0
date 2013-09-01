using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using TypeSystem;
using AssemblyProviders.CSharp;
using AssemblyProviders.CSharp.Compiling;

using UnitTesting.Analyzing_TestUtils;

namespace UnitTesting.TypeSystem_TestUtils
{
    public delegate void ResultAction(AnalyzingResult<MethodID, InstanceInfo> result);

    public class TestingAssembly : AssemblyProvider
    {
        Dictionary<string, MethodItem> _methods = new Dictionary<string, MethodItem>();
        List<EditAction> _editActions = new List<EditAction>();
        List<ResultAction> _userActions = new List<ResultAction>();

        /// <summary>
        /// Simulate actions from user
        /// </summary>
        public IEnumerable<ResultAction> UserActions { get { return _userActions; } }
        public IEnumerable<EditAction> EditActions { get { return _editActions; } }
        
        public TestingAssembly AddMethod(string path, string code, bool isStatic = false,string returnType="", params ParameterInfo[] parameters)
        {
            var info = getInfo(path, isStatic,returnType, parameters);
            var source = new Source("{" + code + "}");
            var method = new ParsedGenerator(info, source);


            addMethod(method, info);
            return this;
        }

        public TestingAssembly AddMethod(string path, DirectMethod<MethodID, InstanceInfo> source, bool isStatic = false, params ParameterInfo[] parameters)
        {
            var info = getInfo(path, isStatic,"", parameters);
            var method = new DirectGenerator(source);

            addMethod(method, info);
            return this;
        }

        public TestingAssembly AddMethod(string path, DirectMethod<MethodID, InstanceInfo> source, string returnType, params ParameterInfo[] parameters)
        {
            var info = getInfo(path, false, returnType, parameters);
            var method = new DirectGenerator(source);

            addMethod(method, info);
            return this;
        }

        public TestingAssembly UserAction(ResultAction action)
        {
            _userActions.Add(action);

            return this;
        }

        public TestingAssembly AddEditAction(string variable, string editName)
        {
            var editAction = new EditAction(new VariableName(variable), editName, null);
            _editActions.Add(editAction);
            return this;
        }

        public string GetSource(string methodPath)
        {
            return (_methods[methodPath].Generator as ParsedGenerator).Source.Code;
        }

        #region Assembly provider implementatation
        public override SearchIterator CreateRootIterator()
        {
            return new HashIterator(_methods);
        }
        protected override string resolveMethod(MethodID method, InstanceInfo[] staticArgumentInfo)
        {
            return method.MethodName;
        }

        protected override GeneratorBase getGenerator(string methodName)
        {
            if (!_methods.ContainsKey(methodName))
            {
                return null;
            }
            var generator = _methods[methodName].Generator;
            var parsedGenerator = generator as ParsedGenerator;
            if (parsedGenerator != null)
            {
                parsedGenerator.SetServices(TypeServices);
            }
            return generator;
        }
        #endregion

        #region Private utils
        private void addMethod(GeneratorBase method, TypeMethodInfo info)
        {
            _methods.Add(info.Path, new MethodItem(method, info));
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
