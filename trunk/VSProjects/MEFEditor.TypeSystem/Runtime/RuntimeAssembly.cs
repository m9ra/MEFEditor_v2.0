using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Linq.Expressions;

using MEFEditor.Drawing;
using MEFEditor.Analyzing;
using MEFEditor.Analyzing.Editing;

using MEFEditor.TypeSystem.DrawingServices;
using MEFEditor.TypeSystem.Runtime.Building;

namespace MEFEditor.TypeSystem.Runtime
{
    /// <summary>
    /// Provider of method generators created from <see cref="RuntimeTypeDefinition"/> and
    /// native methods.
    /// </summary>
    /// <param name="definition">The type definition where method is defined.</param>
    /// <param name="method">The method which generator is requested.</param>
    /// <param name="name">The name of generated method.</param>
    /// <returns>Created generator.</returns>
    delegate RuntimeMethodGenerator GeneratorProvider(RuntimeTypeDefinition definition, MethodInfo method, string name);

    /// <summary>
    /// Assembly that represents Runtime where are loaded
    /// users type definitions. When searching methods
    /// Runtime is always asked first so it can override
    /// behavior of every method.
    /// </summary>
    public class RuntimeAssembly : AssemblyProvider
    {
        /// <summary>
        /// Global edits that are not connected with any <see cref="Instance" /> context.
        /// </summary>
        /// <value>The static edits.</value>
        public IEnumerable<Edit> GlobalEdits { get { return _globalEdits; } }

        /// <summary>
        /// Determine that assembly and its type definitions has been built.
        /// </summary>
        /// <value><c>true</c> if Runtime is built; otherwise, <c>false</c>.</value>
        public bool IsBuilded { get; private set; }

        /// <summary>
        /// Storage of registered methods.
        /// </summary>
        private readonly HashedMethodContainer _runtimeMethods = new HashedMethodContainer();

        /// <summary>
        /// Registered direct types.
        /// </summary>
        private readonly Dictionary<Type, RuntimeTypeDefinition> _directTypes = new Dictionary<Type, RuntimeTypeDefinition>();

        /// <summary>
        /// Direct types in wrapped form.
        /// </summary>
        private readonly HashSet<Type> _wrappedDirectTypes = new HashSet<Type>();

        /// <summary>
        /// Signatures of direct types.
        /// </summary>
        private readonly HashSet<string> _directSignatures = new HashSet<string>();

        /// <summary>
        /// The global edits.
        /// </summary>
        private readonly List<Edit> _globalEdits = new List<Edit>();

        /// <summary>
        /// Registered data types according to their names.
        /// </summary>
        private readonly Dictionary<string, DataTypeDefinition> _dataTypes = new Dictionary<string, DataTypeDefinition>();

        /// <summary>
        /// The available generator providers indexed according method prefix which they provides.
        /// </summary>
        private readonly Dictionary<string, GeneratorProvider> _methodGeneratorProviders;

        /// <summary>
        /// The stored inheritance chains.
        /// </summary>
        private readonly Dictionary<string, InheritanceChain> _inheritanceChains = new Dictionary<string, InheritanceChain>();

        /// <summary>
        /// Full path that is used for current assembly.
        /// </summary>
        private readonly string _fullPath = "//Runtime";

        /// <summary>
        /// Gets the assembly full path.
        /// </summary>
        /// <returns>System.String.</returns>
        protected override string getAssemblyFullPath()
        {
            return _fullPath;
        }

        /// <summary>
        /// Gets the name of the assembly.
        /// </summary>
        /// <returns>System.String.</returns>
        protected override string getAssemblyName()
        {
            return "Runtime";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeAssembly" /> class.
        /// </summary>
        /// <param name="fullPath">The full path.</param>
        public RuntimeAssembly(string fullPath = null)
        {
            if (fullPath != null)
                _fullPath = fullPath;

            _methodGeneratorProviders = new Dictionary<string, GeneratorProvider>()
            {
                {"_method_",_createMethod},
                {"_static_method_",_createStaticMethod},                
                {"_get_",_createProperty},
                {"_static_get",_createStaticProperty},
                {"_set_",_createProperty},
                {"_static_set",_createStaticProperty},
            };

            initializeTypeSystemBase();
        }

        /// <summary>
        /// Initialize important type systems definitions.
        /// </summary>
        private void initializeTypeSystemBase()
        {
            //null support
            var nullDefinition = new DirectTypeDefinition<Null>();
            nullDefinition.ForcedInfo = Null.TypeInfo;
            AddDirectDefinition(nullDefinition);

            //support for System.Object
            var chain = new InheritanceChain(TypeDescriptor.Create<object>(), new InheritanceChain[0]);
            _inheritanceChains.Add(chain.Path.Signature, chain);

            //support for Array
            var arrayDefinition = new DirectTypeDefinition(typeof(Array<>));
            arrayDefinition.ForcedInfo = TypeDescriptor.ArrayInfo;
            AddDirectDefinition(arrayDefinition);
        }

        /// <summary>
        /// Add runtime type definition into runtime assembly.
        /// </summary>
        /// <param name="definition">Added type definition.</param>
        public void AddDefinition(DataTypeDefinition definition)
        {
            _dataTypes.Add(definition.FullName, definition);
        }


        /// <summary>
        /// Adds the direct definition.
        /// </summary>
        /// <param name="definition">The definition.</param>
        public void AddDirectDefinition(DirectTypeDefinition definition)
        {
            _directTypes[definition.DirectType] = definition;
            _wrappedDirectTypes.Add(definition.WrappedDirectType);

            var typeInfo = definition.TypeInfo;
            _directSignatures.Add(new PathInfo(typeInfo.TypeName).Signature);
        }

        /// <summary>
        /// Builds the assembly.
        /// </summary>
        /// <exception cref="System.NotSupportedException">Cannot add definitions until type services are initialized</exception>
        public void BuildAssembly()
        {
            if (IsBuilded)
                //assembly has been already builded
                return;

            if (this.TypeServices == null)
            {
                throw new NotSupportedException("Cannot add definitions until type services are initialized");
            }

            foreach (var directType in _directTypes.Values)
            {
                buildDefinition(directType);
            }

            foreach (var dataType in _dataTypes.Values)
            {
                buildDefinition(dataType);
            }
            IsBuilded = true;
        }

        /// <summary>
        /// Gets the chain.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>InheritanceChain.</returns>
        internal InheritanceChain GetChain(Type type)
        {
            if (type == null)
                return null;

            var path = new PathInfo(type);

            InheritanceChain existingChain;
            if (_inheritanceChains.TryGetValue(path.Signature, out existingChain))
            {
                return existingChain;
            }

            //we have to create new chain
            var subChains = new List<InheritanceChain>();

            var baseChain = GetChain(type.BaseType);
            if (baseChain != null)
                subChains.Add(baseChain);

            foreach (var iface in type.GetInterfaces())
            {
                var subChain = GetChain(iface);
                if (subChain != null)
                    subChains.Add(subChain);
            }

            var info = TypeDescriptor.Create(type);
            var createdChain = new InheritanceChain(info, subChains);

            _inheritanceChains.Add(createdChain.Path.Signature, createdChain);
            return createdChain;
        }


        /// <summary>
        /// Determines whether given type is covered by direct type definitions.
        /// </summary>
        /// <param name="type">The tested type.</param>
        /// <returns><c>true</c> if type is covered; otherwise, <c>false</c>.</returns>
        internal bool IsInDirectCover(Type type)
        {
            return
                type.IsGenericParameter ||
                type == typeof(void) ||
                type.IsArray ||
                typeof(Instance).IsAssignableFrom(type) ||
                type == typeof(InstanceWrap) ||
                _wrappedDirectTypes.Contains(type);
        }

        /// <summary>
        /// Determines whether given type info is covered by direct type definitions.
        /// </summary>
        /// <param name="typeInfo">The tested type information.</param>
        /// <returns><c>true</c> if type is covered; otherwise, <c>false</c>.</returns>
        internal bool IsDirectType(InstanceInfo typeInfo)
        {
            var signature = new PathInfo(typeInfo.TypeName).Signature;
            return _directSignatures.Contains(signature);
        }

        #region Assembly provider implementation

        /// <summary>
        /// Force to load components - suppose that no other components from this assembly are registered.
        /// <remarks>Can be called multiple times when changes in references are registered</remarks>.
        /// </summary>
        protected override void loadComponents()
        {
            foreach (var type in _dataTypes.Values)
            {
                if (type.ComponentInfo != null)
                {
                    ComponentDiscovered(type.ComponentInfo);
                }
            }
        }

        /// <summary>
        /// Creates the root iterator. That is used for
        /// searching method definitions.
        /// </summary>
        /// <returns>SearchIterator.</returns>
        public override SearchIterator CreateRootIterator()
        {
            return new HashedIterator(_runtimeMethods);
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
            return _runtimeMethods.AccordingGenericId(method, searchPath);
        }

        /// <summary>
        /// Gets the method generator for given method identifier.
        /// For performance purposes no generic search has to be done.
        /// </summary>
        /// <param name="method">The method identifier.</param>
        /// <returns>GeneratorBase.</returns>
        public override GeneratorBase GetMethodGenerator(MethodID method)
        {
            return _runtimeMethods.AccordingId(method);
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
            //we may have explicit definition
            alternativeImplementer = null;
            var explicitImplementation = _runtimeMethods.GetImplementation(method, dynamicInfo);
            if (explicitImplementation != null)
                return explicitImplementation;

            //or there is implicit .NET definition
            var signature = PathInfo.GetSignature(dynamicInfo);
            var isNativeObject = _directSignatures.Contains(signature);
            if (!isNativeObject)
                //implicit definition is there only for native objects
                return null;

            //we can use native implementation
            return method; //note that using dynamic flag is a hack
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

            return _runtimeMethods.GetGenericImplementation(methodID, methodSearchPath, implementingTypePath);
        }

        /// <summary>
        /// Gets inheritance chain for type described by given path.
        /// </summary>
        /// <param name="typePath">The type path.</param>
        /// <returns>InheritanceChain.</returns>
        public override InheritanceChain GetInheritanceChain(PathInfo typePath)
        {
            InheritanceChain chain;
            _inheritanceChains.TryGetValue(typePath.Signature, out chain);

            if (chain != null && chain.Type.HasParameters)
            {
                chain = chain.MakeGeneric(typePath.GenericArgs);
            }

            return chain;
        }

        #endregion

        #region TypeDefinition building

        /// <summary>
        /// Register given definition into runtime assembly.
        /// </summary>
        /// <param name="definition">Registered definition.</param>
        private void buildDefinition(RuntimeTypeDefinition definition)
        {
            //every definition needs initialization
            definition.Initialize(this, this.TypeServices);

            //add static edits defined by built definition
            _globalEdits.AddRange(definition.StaticEdits);

            //every definition needs to register its chain
            createChain(definition);

            //get all methods defined by definition
            var methodGenerators = definition.GetMethods();
            foreach (var generator in methodGenerators)
            {
                var item = new MethodItem(generator, generator.MethodInfo);
                _runtimeMethods.AddItem(item, generator.Implemented);
            }

            if (definition.ComponentInfo != null)
            {
                ComponentDiscovered(definition.ComponentInfo);
            }
        }

        /// <summary>
        /// Create inheritance chain for given definition.
        /// </summary>
        /// <param name="definition">Definition which chain is registered.</param>
        private void createChain(RuntimeTypeDefinition definition)
        {
            var subChains = definition.GetSubChains();
            var chain = new InheritanceChain(definition.TypeInfo, subChains);

            registerChain(chain);
        }

        /// <summary>
        /// Register inheritance chain and all its subchains.
        /// </summary>
        /// <param name="chain">Registered chain.</param>
        private void registerChain(InheritanceChain chain)
        {
            _inheritanceChains[chain.Path.Signature] = chain;

            foreach (var subchain in chain.SubChains)
            {
                registerChain(subchain);
            }
        }

        /// <summary>
        /// Get method generators defined in given runtime type definition.
        /// </summary>
        /// <param name="definition">Definition which methods are resolved.</param>
        /// <returns>Method generators for defined methods.</returns>
        internal IEnumerable<RuntimeMethodGenerator> GetMethodGenerators(RuntimeTypeDefinition definition)
        {
            var result = new List<RuntimeMethodGenerator>();
            foreach (var method in definition.GetType().GetMethods())
            {
                var name = method.Name;

                //create method definition according to it's prefix
                foreach (var provider in _methodGeneratorProviders)
                {
                    if (name.StartsWith(provider.Key))
                    {
                        var notPrefixedName = name.Substring(provider.Key.Length);
                        var generator = provider.Value(definition, method, notPrefixedName);
                        result.Add(generator);
                        break;
                    }
                }
            }

            return result;
        }

        #endregion

        #region Runtime method building

        /// <summary>
        /// Build method generator from given method info.
        /// </summary>
        /// <param name="definition">Type where method is defined.</param>
        /// <param name="method">Method info defining method.</param>
        /// <param name="methodName">Name of defined method.</param>
        /// <param name="forceStatic">if set to <c>true</c> [force static].</param>
        /// <returns>Builder where method is built.</returns>
        private RuntimeMethodGenerator buildMethod(RuntimeTypeDefinition definition, MethodInfo method, string methodName, bool forceStatic = false)
        {
            var builder = new MethodBuilder(definition, methodName, forceStatic);
            builder.ThisObjectExpression = builder.DeclaringDefinitionConstant;
            builder.AdapterFor(method);
            return builder.Build();
        }

        /// <summary>
        /// Determine, that given parameter needs wrapping.
        /// </summary>
        /// <param name="par">Parameter which wrapping is determined.</param>
        /// <returns>True if parameter needs wrapping, false otherwise.</returns>
        private bool needWrapping(System.Reflection.ParameterInfo par)
        {
            var parType = par.ParameterType;
            var instType = typeof(Instance);

            var isInstance = instType == parType;
            var hasInstanceParent = instType.IsSubclassOf(parType);


            return !isInstance && !hasInstanceParent;
        }

        #endregion

        #region Method generator providers

        /// <summary>
        /// Creates the method.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="method">The method.</param>
        /// <param name="name">The name.</param>
        /// <returns>RuntimeMethodGenerator.</returns>
        private RuntimeMethodGenerator _createMethod(RuntimeTypeDefinition definition, MethodInfo method, string name)
        {
            if (name == "ctor")
            {
                name = Naming.CtorName;
            }
            var generator = buildMethod(definition, method, name);

            return generator;
        }

        /// <summary>
        /// Creates the static method.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="method">The method.</param>
        /// <param name="name">The name.</param>
        /// <returns>RuntimeMethodGenerator.</returns>
        private RuntimeMethodGenerator _createStaticMethod(RuntimeTypeDefinition definition, MethodInfo method, string name)
        {
            if (name == "cctor")
            {
                name = Naming.ClassCtorName;
            }

            var generator = buildMethod(definition, method, name, true);
            return generator;
        }

        /// <summary>
        /// Creates the property.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="method">The method.</param>
        /// <param name="name">The name.</param>
        /// <returns>RuntimeMethodGenerator.</returns>
        private RuntimeMethodGenerator _createProperty(RuntimeTypeDefinition definition, MethodInfo method, string name)
        {
            var generator = buildMethod(definition, method, method.Name.Substring(1));

            return generator;
        }

        /// <summary>
        /// Creates the static property.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="method">The method.</param>
        /// <param name="name">The name.</param>
        /// <returns>RuntimeMethodGenerator.</returns>
        private RuntimeMethodGenerator _createStaticProperty(RuntimeTypeDefinition definition, MethodInfo method, string name)
        {
            var prefix = "_static_";
            var generator = buildMethod(definition, method, method.Name.Substring(prefix.Length), true);

            return generator;
        }

        #endregion

        /// <summary>
        /// Gets type definition that can provide drawing of given instance.
        /// </summary>
        /// <param name="instance">The instance that will be drawn.</param>
        /// <returns>Type definition that can provide drawing if available, <c>null</c> otherwise.</returns>
        public DataTypeDefinition GetDrawer(Instance instance)
        {
            DataTypeDefinition typeDefinition;
            _dataTypes.TryGetValue(instance.Info.TypeName, out typeDefinition);
            //TODO resolve generic types and inheritance
            return typeDefinition;
        }

        /// <summary>
        /// Gets the type definition for type of given instance.
        /// </summary>
        /// <param name="instance">The instance which type definition is requested.</param>
        /// <returns>Type definition if available, <c>null</c> otherwise.</returns>
        internal RuntimeTypeDefinition GetTypeDefinition(Instance instance)
        {
            DataTypeDefinition typeDefinition;
            _dataTypes.TryGetValue(instance.Info.TypeName, out typeDefinition);

            return typeDefinition;
        }

        /// <summary>
        /// Creates the drawing pipeline that is used for drawing processing.
        /// Pipeline is used therefore drawing of every instance consists of multiple steps.
        /// </summary>
        /// <param name="drawer">The drawer of general definitions that is used for every drawn instance.</param>
        /// <param name="result">The result of analysis which instances are drawn.</param>
        /// <returns>Created pipeline.</returns>
        public DrawingPipeline CreateDrawingPipeline(GeneralDrawer drawer, AnalyzingResult result)
        {
            return new DrawingPipeline(drawer, this, result);
        }

        /// <summary>
        /// Runs the edit on given view, in context of corresponding type definition.
        /// </summary>
        /// <param name="edit">The edit that will be processed in the given view.</param>
        /// <param name="view">The view where edit will be processed.</param>
        /// <returns>View representation compatible with <see cref="MEFEditor.Drawing"/> library.</returns>
        public EditViewBase RunEdit(Edit edit, EditView view)
        {
            var creatorDefinition = GetTypeDefinition(edit.Creator);

            EditViewBase result = null;
            Action editRun = () =>
            {
                result = view.Apply(edit.Transformation);
            };

            if (creatorDefinition == null)
            {
                //there is no available definition
                editRun();
            }
            else
            {
                creatorDefinition.RunInContextOf(edit.Creator, editRun, edit.Context);
            }

            return result;
        }
    }
}
