using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Linq.Expressions;

using Analyzing;
using Analyzing.Editing;
using TypeSystem;
using TypeSystem.Runtime;

using Mono.Cecil;

using AssemblyProviders.CSharp;
using AssemblyProviders.CSharp.Compiling;
using AssemblyProviders.CIL;

using UnitTesting.Analyzing_TestUtils;
using UnitTesting.Analyzing_TestUtils.Environment;
using UnitTesting.AssemblyProviders_TestUtils;

namespace UnitTesting.TypeSystem_TestUtils
{
    public delegate void ResultAction(AnalyzingResult result);

    public delegate Drawing.ContentDrawing DrawingCreator(Drawing.DiagramItem item);

    public class TestingAssembly : AssemblyProvider
    {
        /// <summary>
        /// Methods contained in current assembly
        /// </summary>
        private readonly HashedMethodContainer _methods = new HashedMethodContainer();

        /// <summary>
        /// Testing simulation of edit actions
        /// </summary>
        private readonly List<EditAction> _editActions = new List<EditAction>();

        /// <summary>
        /// Testing simulation of user actions
        /// </summary>
        private readonly List<ResultAction> _userActions = new List<ResultAction>();

        /// <summary>
        /// Actions that are processed before runtime build
        /// </summary>
        private readonly List<Action> _beforeRuntimeBuildActions = new List<Action>();

        /// <summary>
        /// Actions that are processed after runtime builded
        /// </summary>
        private readonly List<Action> _afterRuntimeActions = new List<Action>();

        /// <summary>
        /// Factory which will be used fo "loading" providers
        /// </summary>
        private readonly SimpleAssemblyFactory _factory = new SimpleAssemblyFactory();

        /// <summary>
        /// Inheritance rules that are known within assembly
        /// </summary>
        private readonly Dictionary<TypeDescriptor, TypeDescriptor> _knownInheritance = new Dictionary<TypeDescriptor, TypeDescriptor>();

        /// <summary>
        /// Registered drawing providers according their types
        /// </summary>
        public readonly Dictionary<string, DrawingCreator> RegisteredDrawers = new Dictionary<string, DrawingCreator>();

        /// <summary>
        /// Method loader used by assembly
        /// </summary>
        public readonly AssemblyLoader Loader;

        /// <summary>
        /// because of accessing runtime adding services for testing purposes
        /// </summary>
        public readonly RuntimeAssembly Runtime;

        /// <summary>
        /// Settings available for machine
        /// </summary>
        public readonly MachineSettings Settings;

        /// <summary>
        /// Current application domain
        /// </summary>
        public AppDomainServices AppDomain { get { return Loader.AppDomain; } }

        /// <summary>
        /// Testing simulation of user actions
        /// </summary>
        public IEnumerable<ResultAction> UserActions { get { return _userActions; } }

        /// <summary>
        /// Testing simulation of edit actions
        /// </summary>
        public IEnumerable<EditAction> EditActions { get { return _editActions; } }

        /// <summary>
        /// Current machine
        /// </summary>
        public readonly Machine Machine;

        /// <summary>
        /// Determine that assembly has been already builded. Methods can be added even after builded.
        /// </summary>
        public bool IsBuilded { get; private set; }

        public TestingAssembly(MachineSettings settings)
        {
            Settings = settings;
            Runtime = settings.Runtime;
            Machine = SettingsProvider.CreateMachine(Settings);

            Loader = new AssemblyLoader(Settings, _factory);

            //load self
            _factory.Register(this, this);
            Loader.LoadRoot(this);
        }

        public void Build()
        {
            if (IsBuilded)
                throw new NotSupportedException("Runtime can't be builded");

            IsBuilded = true;

            foreach (var beforeAction in _beforeRuntimeBuildActions)
            {
                beforeAction();
            }

            Runtime.BuildAssembly();

            foreach (var afterAction in _afterRuntimeActions)
            {
                afterAction();
            }
        }

        #region Adding methods to current assembly

        public TestingAssembly AddMethodRaw(string methodPath, string rawCode, MethodDescription description)
        {
            var methodInfo = buildDescription(description, methodPath);
            var genericParameters = new PathInfo(methodPath).GenericArgs;

            var method = new ParsedGenerator(methodInfo, rawCode, genericParameters, TypeServices);

            addMethod(method, methodInfo, description.Implemented);

            return this;
        }

        public TestingAssembly AddMethod(string methodPath, string code, MethodDescription description)
        {
            var sourceCode = "{" + code + "}";
            return AddMethodRaw(methodPath, sourceCode, description);
        }

        public TestingAssembly AddMethod(string methodPath, DirectMethod source, MethodDescription description)
        {
            var methodInfo = buildDescription(description, methodPath);

            var method = new DirectGenerator(source);
            addMethod(method, methodInfo, description.Implemented);

            return this;
        }

        public TestingAssembly AddMethod(string methodPath, MethodInfo sourceMethod, MethodDescription description)
        {
            var methodInfo = buildDescription(description, methodPath);

            var source = new CILMethod(sourceMethod, methodInfo);
            var method = new CILGenerator(methodInfo, source, TypeServices);
            addMethod(method, methodInfo, description.Implemented);

            return this;
        }

        public TestingAssembly AddMethod(string methodPath, MethodDefinition sourceMethod, MethodDescription description)
        {
            var methodInfo = buildDescription(description, methodPath);

            var source = new CILMethod(sourceMethod, methodInfo);
            var method = new CILGenerator(methodInfo, source, TypeServices);
            addMethod(method, methodInfo, description.Implemented);

            return this;
        }

        #endregion

        #region Assembly reference handling

        public TestingAssembly AddAssembly(AssemblyProvider testAssembly)
        {
            afterRuntimeAction(() =>
            {
                addRootAssembly(testAssembly);
                var runtime = testAssembly as RuntimeAssembly;
                if (runtime != null)
                    runtime.BuildAssembly();
            });

            return this;
        }

        public TestingAssembly RemoveAssembly(AssemblyProvider assembly)
        {
            afterRuntimeAction(() =>
            {
                removeRootAssembly(assembly);
            });

            return this;
        }

        #endregion

        #region Runtime preparation

        public TestingAssembly AddToRuntime<T>()
            where T : DataTypeDefinition
        {
            beforeRuntimeAction(() =>
            {
                var runtimeTypeDef = Activator.CreateInstance<T>();
                Runtime.AddDefinition(runtimeTypeDef);
            });

            return this;
        }

        public TestingAssembly AddToRuntime<T, D>()
            where T : DataTypeDefinition
            where D : Drawing.ContentDrawing
        {
            beforeRuntimeAction(() =>
            {
                var runtimeTypeDef = Activator.CreateInstance<T>();
                Runtime.AddDefinition(runtimeTypeDef);
                RegisterDrawing<D>(runtimeTypeDef.TypeInfo.TypeName);
            });

            return this;
        }

        public TestingAssembly RegisterDrawing<D>(string registeredTypeName)
            where D : Drawing.ContentDrawing
        {
            //item that is passed as argument constructor
            var itemType = typeof(Drawing.DiagramItem);

            //parameter for constructor
            var itemParameter = Expression.Parameter(itemType, "item");
            //constructor call
            var newCall = Expression.New(typeof(D).GetConstructor(new[] { itemType }), new[] { itemParameter });

            //compile constructor
            var provider = Expression.Lambda<DrawingCreator>(newCall, itemParameter).Compile();

            //register provider            
            RegisteredDrawers.Add(registeredTypeName, provider);

            return this;
        }


        public TestingAssembly AddDirectToRuntime<T>()
        {
            beforeRuntimeAction(() =>
            {
                SettingsProvider.AddDirectType(Runtime, typeof(T));
            });

            return this;
        }

        /// <summary>
        /// Generic parameters has to be satisfiable by Instance
        /// </summary>
        /// <param name="genericType">Type which generic arguments will be substituted by WrappedInstance</param>
        /// <returns></returns>
        public TestingAssembly AddWrappedGenericToRuntime(Type genericType)
        {
            beforeRuntimeAction(() =>
            {
                SettingsProvider.AddDirectType(Runtime, genericType);
            });

            return this;
        }

        #endregion

        #region Testing Simulation of user IO

        public TestingAssembly UserAction(ResultAction action)
        {
            _userActions.Add(action);

            return this;
        }

        public TestingAssembly RunEditAction(string variable, string editName)
        {
            var editAction = EditAction.Edit(new VariableName(variable), editName);
            _editActions.Add(editAction);
            return this;
        }

        public TestingAssembly RunRemoveAction(string variable)
        {
            var editAction = EditAction.Remove(new VariableName(variable));
            _editActions.Add(editAction);
            return this;
        }

        public string GetSource(MethodID method, ExecutionView view)
        {
            var parsedGenerator = _methods.AccordingId(method) as ParsedGenerator;

            if (parsedGenerator == null)
                return "Source not available for " + method;

            return parsedGenerator.Source.GetCode(view);
        }

        public void SetSource(MethodID method, string sourceCode)
        {
            var name = Naming.GetMethodPath(method).Name;
            ReportInvalidation(name);
            var parsedGenerator = _methods.AccordingId(method) as ParsedGenerator;

            var newGenerator = parsedGenerator.ChangeSource(sourceCode);
            var newMethod = new MethodItem(newGenerator, newGenerator.Method);

            _methods.RemoveItem(method);
            _methods.AddItem(newMethod, TypeDescriptor.NoDescriptors);
        }

        #endregion

        #region Assembly provider implementatation

        /// <inheritdoc />
        protected override void loadComponents()
        {
            //probably TestAssembly doesnt need to load assemblies
        }

        protected override string getAssemblyFullPath()
        {
            return "//TestingAssembly";
        }

        protected override string getAssemblyName()
        {
            return "TestingAssembly";
        }

        public override SearchIterator CreateRootIterator()
        {
            requireBuilded();
            return new HashedIterator(_methods);
        }

        public override GeneratorBase GetMethodGenerator(MethodID method)
        {
            requireBuilded();
            return _methods.AccordingId(method);
        }

        public override GeneratorBase GetGenericMethodGenerator(MethodID method, PathInfo searchPath)
        {
            requireBuilded();
            return _methods.AccordingGenericId(method, searchPath);
        }

        public override MethodID GetImplementation(MethodID method, TypeDescriptor dynamicInfo, out TypeDescriptor alternativeImplementer)
        {
            alternativeImplementer = null;

            requireBuilded();
            return _methods.GetImplementation(method, dynamicInfo);
        }

        public override MethodID GetGenericImplementation(MethodID methodID, PathInfo methodSearchPath, PathInfo implementingTypePath, out PathInfo alternativeImplementer)
        {
            alternativeImplementer = null;

            requireBuilded();
            return _methods.GetGenericImplementation(methodID, methodSearchPath, implementingTypePath);
        }

        public override InheritanceChain GetInheritanceChain(PathInfo typePath)
        {
            requireBuilded();

            var descriptor = TypeDescriptor.Create(typePath.Name);

            if (!_knownInheritance.ContainsKey(descriptor))
                //we handle only contained types
                return null;

            //inheritance according to known definitions
            return TypeServices.CreateChain(descriptor, new[] { TypeServices.GetChain(_knownInheritance[descriptor]) });
        }

        #endregion

        #region Private utils

        private void addRootAssembly(AssemblyProvider assembly)
        {
            //register assembly
            _factory.Register(assembly.FullPath, assembly);

            //load assembly
            Loader.LoadRoot(assembly.FullPath);
        }

        private void removeRootAssembly(AssemblyProvider assembly)
        {
            throw new NotImplementedException();
        }

        private void requireBuilded()
        {
            if (!Runtime.IsBuilded)
            {
                throw new NotSupportedException("Operation cannot be processed when assembly is not builded");
            }
        }

        private void afterRuntimeAction(Action action)
        {
            if (Runtime.IsBuilded)
            {
                // runtime is builded action can be done
                action();
            }
            else
            {
                //wait until runtime is builded
                _afterRuntimeActions.Add(action);
            }
        }

        private void beforeRuntimeAction(Action action)
        {
            if (Runtime.IsBuilded)
                throw new NotSupportedException("Cannot add action after runtime is builded");

            _beforeRuntimeBuildActions.Add(action);
        }

        private TypeMethodInfo buildDescription(MethodDescription description, string methodPath)
        {
            var info = description.CreateInfo(methodPath);
            return info;
        }

        private void addMethod(GeneratorBase method, TypeMethodInfo info, IEnumerable<InstanceInfo> implementedTypes)
        {
            var implemented = implementedTypes.ToArray();

            if (!_knownInheritance.ContainsKey(info.DeclaringType))
                _knownInheritance[info.DeclaringType] = TypeDescriptor.ObjectInfo;

            _methods.AddItem(new MethodItem(method, info), implemented);
        }

        #endregion


        internal TestingAssembly DefineInheritance(string childType, Type parentType)
        {
            var childDescriptor = TypeDescriptor.Create(childType);
            var parentDescriptor = TypeDescriptor.Create(parentType);

            _knownInheritance[childDescriptor] = parentDescriptor;

            return this;
        }
    }
}
