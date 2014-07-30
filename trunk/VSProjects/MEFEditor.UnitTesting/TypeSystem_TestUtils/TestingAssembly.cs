using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Linq.Expressions;

using Mono.Cecil;

using MEFEditor.Analyzing;
using MEFEditor.Analyzing.Editing;
using MEFEditor.TypeSystem;
using MEFEditor.TypeSystem.Runtime;

using RecommendedExtensions.Core.Languages.CSharp;
using RecommendedExtensions.Core.Languages.CSharp.Compiling;
using RecommendedExtensions.Core.Languages.CIL;

using MEFEditor.UnitTesting.Analyzing_TestUtils;
using MEFEditor.UnitTesting.Analyzing_TestUtils.Environment;
using MEFEditor.UnitTesting.AssemblyProviders_TestUtils;

namespace MEFEditor.UnitTesting.TypeSystem_TestUtils
{
    /// <summary>
    /// Testing action on analysis result.
    /// </summary>
    /// <param name="result">Analysis result.</param>
    public delegate void ResultAction(AnalyzingResult result);

    /// <summary>
    /// Creator of content drawings.
    /// </summary>
    /// <param name="item">Item which drawing will be created.</param>
    /// <returns>Created content drawing.</returns>
    public delegate MEFEditor.Drawing.ContentDrawing DrawingCreator(MEFEditor.Drawing.DiagramItem item);

    /// <summary>
    /// Implements <see cref="AssemblyProvider"/> that can be used by testing framework.
    /// It provides many utility methods which can test different parts of MEFEditor.
    /// 
    /// Testing workflow looks like creating <see cref="TestingAssembly"/>, setting testing environment 
    /// through <see cref="TestingAssembly"/> methods and asserting evaluation of results.
    /// </summary>
    public class TestingAssembly : AssemblyProvider
    {
        /// <summary>
        /// Methods contained in current assembly.
        /// </summary>
        private readonly HashedMethodContainer _methods = new HashedMethodContainer();

        /// <summary>
        /// Testing simulation of edit actions.
        /// </summary>
        private readonly List<EditAction> _editActions = new List<EditAction>();

        /// <summary>
        /// Testing simulation of user actions.
        /// </summary>
        private readonly List<ResultAction> _userActions = new List<ResultAction>();

        /// <summary>
        /// Actions that are processed before runtime build.
        /// </summary>
        private readonly List<Action> _beforeRuntimeBuildActions = new List<Action>();

        /// <summary>
        /// Actions that are processed after runtime built.
        /// </summary>
        private readonly List<Action> _afterRuntimeActions = new List<Action>();

        /// <summary>
        /// Factory which will be used fo "loading" providers.
        /// </summary>
        private readonly SimpleAssemblyFactory _factory = new SimpleAssemblyFactory();

        /// <summary>
        /// Inheritance rules that are known within assembly.
        /// </summary>
        private readonly Dictionary<TypeDescriptor, TypeDescriptor> _knownInheritance = new Dictionary<TypeDescriptor, TypeDescriptor>();

        /// <summary>
        /// Registered drawing providers according their types.
        /// </summary>
        public readonly Dictionary<string, DrawingCreator> RegisteredDrawers = new Dictionary<string, DrawingCreator>();

        /// <summary>
        /// Method loader used by assembly.
        /// </summary>
        public readonly AssemblyLoader Loader;

        /// <summary>
        /// Runtime used for testing
        /// </summary>
        public readonly RuntimeAssembly Runtime;

        /// <summary>
        /// Settings available for machine.
        /// </summary>
        public readonly MachineSettings Settings;

        /// <summary>
        /// Current application domain.
        /// </summary>
        /// <value>The application domain.</value>
        public AppDomainServices AppDomain { get { return Loader.AppDomain; } }

        /// <summary>
        /// Testing simulation of user actions.
        /// </summary>
        /// <value>The user actions.</value>
        public IEnumerable<ResultAction> UserActions { get { return _userActions; } }

        /// <summary>
        /// Testing simulation of edit actions.
        /// </summary>
        /// <value>The edit actions.</value>
        public IEnumerable<EditAction> EditActions { get { return _editActions; } }

        /// <summary>
        /// Current machine.
        /// </summary>
        public readonly Machine Machine;

        /// <summary>
        /// Determine that assembly has been already builded. Methods can be added even after builded.
        /// </summary>
        /// <value><c>true</c> if this instance is builded; otherwise, <c>false</c>.</value>
        public bool IsBuilded { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestingAssembly" /> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public TestingAssembly(MachineSettings settings)
        {
            Settings = settings;
            Runtime = settings.Runtime;
            Machine = SettingsProvider.CreateMachine(Settings);

            Loader = new AssemblyLoader(Settings, _factory);

            //load self
            _factory.Register(this, this);
            Loader.AppDomain.Transactions.StartNew("Test");
            Loader.LoadRoot(this);
        }

        /// <summary>
        /// Builds current testing assembly.
        /// </summary>
        /// <exception cref="System.NotSupportedException">Runtime can't be built</exception>
        public void Build()
        {
            if (IsBuilded)
                throw new NotSupportedException("Runtime can't be built");

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

        /// <summary>
        /// Adds the method defined by its raw C# source code (those that doesn't contain
        /// any additional brackets,..)
        /// </summary>
        /// <param name="methodPath">The method path.</param>
        /// <param name="rawCode">The raw code.</param>
        /// <param name="description">The description of method signature.</param>
        /// <returns>TestingAssembly.</returns>
        public TestingAssembly AddMethodRaw(string methodPath, string rawCode, MethodDescription description)
        {
            var methodInfo = buildDescription(description, methodPath);
            var genericParameters = new PathInfo(methodPath).GenericArgs;

            var method = new ParsedGenerator(methodInfo, rawCode, genericParameters, TypeServices);

            addMethod(method, methodInfo, description.Implemented);

            return this;
        }

        /// <summary>
        /// Adds the method defined by its C# source code.
        /// </summary>
        /// <param name="methodPath">The method path.</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description of method signature.</param>
        /// <returns>TestingAssembly.</returns>
        public TestingAssembly AddMethod(string methodPath, string code, MethodDescription description)
        {
            var sourceCode = "{" + code + "}";
            return AddMethodRaw(methodPath, sourceCode, description);
        }

        /// <summary>
        /// Adds the method defined by direct native method.
        /// </summary>
        /// <param name="methodPath">The method path.</param>
        /// <param name="source">The source.</param>
        /// <param name="description">The description of method signature.</param>
        /// <returns>TestingAssembly.</returns>
        public TestingAssembly AddMethod(string methodPath, DirectMethod source, MethodDescription description)
        {
            var methodInfo = buildDescription(description, methodPath);

            var method = new DirectGenerator(source);
            addMethod(method, methodInfo, description.Implemented);

            return this;
        }

        /// <summary>
        /// Adds the method defined by its CIL (native) representation.
        /// </summary>
        /// <param name="methodPath">The method path.</param>
        /// <param name="sourceMethod">The source method.</param>
        /// <param name="description">The description of method signature.</param>
        /// <returns>TestingAssembly.</returns>
        public TestingAssembly AddMethod(string methodPath, MethodInfo sourceMethod, MethodDescription description)
        {
            var methodInfo = buildDescription(description, methodPath);

            var source = new CILMethod(sourceMethod, methodInfo);
            var method = new CILGenerator(methodInfo, source, TypeServices);
            addMethod(method, methodInfo, description.Implemented);

            return this;
        }

        /// <summary>
        /// Adds the method defiend by its CECIL (Mono.Cecil) representation.
        /// </summary>
        /// <param name="methodPath">The method path.</param>
        /// <param name="sourceMethod">The source method.</param>
        /// <param name="description">The description of method signature.</param>
        /// <returns>TestingAssembly.</returns>
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

        /// <summary>
        /// Adds specified assembly to the AppDomain.
        /// </summary>
        /// <param name="testAssembly">The test assembly.</param>
        /// <returns>TestingAssembly.</returns>
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

        /// <summary>
        /// Removes the assembly from AppDomain.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>TestingAssembly.</returns>
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

        /// <summary>
        /// Adds <see cref="DataTypeDefinition"/> to Runtime.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>TestingAssembly.</returns>
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

        /// <summary>
        /// Adds <see cref="DataTypeDefinition"/> with defined content drawing to Runtime.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="D"></typeparam>
        /// <returns>TestingAssembly.</returns>
        public TestingAssembly AddToRuntime<T, D>()
            where T : DataTypeDefinition
            where D : MEFEditor.Drawing.ContentDrawing
        {
            beforeRuntimeAction(() =>
            {
                var runtimeTypeDef = Activator.CreateInstance<T>();
                Runtime.AddDefinition(runtimeTypeDef);
                RegisterDrawing<D>(runtimeTypeDef.TypeInfo.TypeName);
            });

            return this;
        }

        /// <summary>
        /// Registers the drawing for given type.
        /// </summary>
        /// <typeparam name="D"></typeparam>
        /// <param name="registeredTypeName">Name of the registered type.</param>
        /// <returns>TestingAssembly.</returns>
        public TestingAssembly RegisterDrawing<D>(string registeredTypeName)
            where D : MEFEditor.Drawing.ContentDrawing
        {
            //item that is passed as argument constructor
            var itemType = typeof(MEFEditor.Drawing.DiagramItem);

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


        /// <summary>
        /// Adds the type as <see cref="DirectTypeDefinition"/> to runtime.
        /// </summary>
        /// <typeparam name="T">Type that will be added</typeparam>
        /// <returns>TestingAssembly.</returns>
        public TestingAssembly AddDirectToRuntime<T>()
        {
            beforeRuntimeAction(() =>
            {
                SettingsProvider.AddDirectType(Runtime, typeof(T));
            });

            return this;
        }

        /// <summary>
        /// Generic parameters has to be satisfiable by Instance.
        /// </summary>
        /// <param name="genericType">Type which generic arguments will be substituted by WrappedInstance.</param>
        /// <returns>TestingAssembly.</returns>
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

        /// <summary>
        /// Represents action from user.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>TestingAssembly.</returns>
        public TestingAssembly UserAction(ResultAction action)
        {
            _userActions.Add(action);

            return this;
        }

        /// <summary>
        /// Runs the edit action on <see cref="Instance"/> defined by given variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="editName">Name of the edit.</param>
        /// <returns>TestingAssembly.</returns>
        public TestingAssembly RunEditAction(string variable, string editName)
        {
            var editAction = EditAction.Edit(new VariableName(variable), editName);
            _editActions.Add(editAction);
            return this;
        }

        /// <summary>
        /// Runs the remove action on <see cref="Instance"/> defined by given variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>TestingAssembly.</returns>
        public TestingAssembly RunRemoveAction(string variable)
        {
            var editAction = EditAction.Remove(new VariableName(variable));
            _editActions.Add(editAction);
            return this;
        }

        /// <summary>
        /// Gets the source of method with given <see cref="MethodID"/>.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="view">The view.</param>
        /// <returns>System.String.</returns>
        public string GetSource(MethodID method, ExecutionView view)
        {
            var parsedGenerator = _methods.AccordingId(method) as ParsedGenerator;

            if (parsedGenerator == null)
                return "Source not available for " + method;

            return parsedGenerator.Source.GetCode(view);
        }

        /// <summary>
        /// Sets the source of method with given <see cref="MethodID"/>.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="sourceCode">The source code.</param>
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

        /// <summary>
        /// Force to load components - suppose that no other components from this assembly are registered.
        /// <remarks>Can be called multiple times when changes in references are registered</remarks>.
        /// </summary>
        /// <inheritdoc />
        protected override void loadComponents()
        {
            //probably TestAssembly doesnt need to load assemblies
        }

        /// <summary>
        /// Gets the assembly full path.
        /// </summary>
        /// <returns>System.String.</returns>
        protected override string getAssemblyFullPath()
        {
            return "//TestingAssembly";
        }

        /// <summary>
        /// Gets the name of the assembly.
        /// </summary>
        /// <returns>System.String.</returns>
        protected override string getAssemblyName()
        {
            return "TestingAssembly";
        }

        /// <summary>
        /// Creates the root iterator. That is used for
        /// searching method definitions.
        /// </summary>
        /// <returns>SearchIterator.</returns>
        public override SearchIterator CreateRootIterator()
        {
            requireBuilded();
            return new HashedIterator(_methods);
        }

        /// <summary>
        /// Gets the method generator for given method identifier.
        /// For performance purposes no generic search has to be done.
        /// </summary>
        /// <param name="method">The method identifier.</param>
        /// <returns>GeneratorBase.</returns>
        public override GeneratorBase GetMethodGenerator(MethodID method)
        {
            requireBuilded();
            return _methods.AccordingId(method);
        }

        /// <summary>
        /// Gets the generic method generator for given method identifier.
        /// Generic has to be resolved according to given search path.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="searchPath">The search path.</param>
        /// <returns>GeneratorBase.</returns>
        public override GeneratorBase GetGenericMethodGenerator(MethodID method, PathInfo searchPath)
        {
            requireBuilded();
            return _methods.AccordingGenericId(method, searchPath);
        }

        /// <summary>
        /// Gets identifier of implementing method for given abstract method.
        /// </summary>
        /// <param name="method">The abstract method identifier.</param>
        /// <param name="dynamicInfo">The dynamic information.</param>
        /// <param name="alternativeImplementer">The alternative implementer which can define requested method.</param>
        /// <returns>Identifier of implementing method.</returns>
        public override MethodID GetImplementation(MethodID method, TypeDescriptor dynamicInfo, out TypeDescriptor alternativeImplementer)
        {
            alternativeImplementer = null;

            requireBuilded();
            return _methods.GetImplementation(method, dynamicInfo);
        }

        /// <summary>
        /// Gets identifier of implementing method for given abstract method.
        /// </summary>
        /// <param name="methodID">The abstract method identifier.</param>
        /// <param name="methodSearchPath">The method search path.</param>
        /// <param name="implementingTypePath">The implementing type path.</param>
        /// <param name="alternativeImplementer">The alternative implementer which can define requested method.</param>
        /// <returns>Identifier of implementing method.</returns>
        public override MethodID GetGenericImplementation(MethodID methodID, PathInfo methodSearchPath, PathInfo implementingTypePath, out PathInfo alternativeImplementer)
        {
            alternativeImplementer = null;

            requireBuilded();
            return _methods.GetGenericImplementation(methodID, methodSearchPath, implementingTypePath);
        }

        /// <summary>
        /// Gets inheritance chain for type described by given path.
        /// </summary>
        /// <param name="typePath">The type path.</param>
        /// <returns>InheritanceChain.</returns>
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

        /// <summary>
        /// Adds the root assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        private void addRootAssembly(AssemblyProvider assembly)
        {
            //register assembly
            _factory.Register(assembly.FullPath, assembly);

            //load assembly
            Loader.LoadRoot(assembly.FullPath);
        }

        /// <summary>
        /// Removes the root assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void removeRootAssembly(AssemblyProvider assembly)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Requires the builded.
        /// </summary>
        /// <exception cref="System.NotSupportedException">Operation cannot be processed when assembly is not builded</exception>
        private void requireBuilded()
        {
            if (!Runtime.IsBuilded)
            {
                throw new NotSupportedException("Operation cannot be processed when assembly is not builded");
            }
        }

        /// <summary>
        /// Afters the runtime action.
        /// </summary>
        /// <param name="action">The action.</param>
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

        /// <summary>
        /// Befores the runtime action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <exception cref="System.NotSupportedException">Cannot add action after runtime is builded</exception>
        private void beforeRuntimeAction(Action action)
        {
            if (Runtime.IsBuilded)
                throw new NotSupportedException("Cannot add action after runtime is builded");

            _beforeRuntimeBuildActions.Add(action);
        }

        /// <summary>
        /// Builds the description.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="methodPath">The method path.</param>
        /// <returns>TypeMethodInfo.</returns>
        private TypeMethodInfo buildDescription(MethodDescription description, string methodPath)
        {
            var info = description.CreateInfo(methodPath);
            return info;
        }

        /// <summary>
        /// Adds the method.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="info">The information.</param>
        /// <param name="implementedTypes">The implemented types.</param>
        private void addMethod(GeneratorBase method, TypeMethodInfo info, IEnumerable<InstanceInfo> implementedTypes)
        {
            var implemented = implementedTypes.ToArray();

            if (!_knownInheritance.ContainsKey(info.DeclaringType))
                _knownInheritance[info.DeclaringType] = TypeDescriptor.ObjectInfo;

            _methods.AddItem(new MethodItem(method, info), implemented);
        }

        #endregion
        
        /// <summary>
        /// Defines the inheritance of given childType.
        /// </summary>
        /// <param name="childType">Type of the child.</param>
        /// <param name="parentType">Type of the parent.</param>
        /// <returns>TestingAssembly.</returns>
        internal TestingAssembly DefineInheritance(string childType, Type parentType)
        {
            var childDescriptor = TypeDescriptor.Create(childType);
            var parentDescriptor = TypeDescriptor.Create(parentType);

            _knownInheritance[childDescriptor] = parentDescriptor;

            return this;
        }
    }
}
