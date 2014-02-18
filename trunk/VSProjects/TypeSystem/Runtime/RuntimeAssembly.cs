using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Linq.Expressions;

using Drawing;
using Analyzing;
using Analyzing.Editing;

using TypeSystem.DrawingServices;
using TypeSystem.Runtime.Building;

namespace TypeSystem.Runtime
{
    delegate RuntimeMethodGenerator GeneratorProvider(RuntimeTypeDefinition definition, MethodInfo method, string name);

    public class RuntimeAssembly : AssemblyProvider
    {
        /// <summary>
        /// TODO: Correct generic typing for array
        /// </summary>
        public static readonly TypeDescriptor ArrayInfo = TypeDescriptor.Create("Array<@0,@1>");

        /// <summary>
        /// Type descriptor for object
        /// </summary>
        public static readonly TypeDescriptor ObjectInfo = TypeDescriptor.Create<object>();

        /// <summary>
        /// Static edits that are available without instance context
        /// </summary>
        public IEnumerable<Edit> StaticEdits { get { return _staticEdits; } }

        /// <summary>
        /// Determine that assembly has been builded
        /// </summary>
        private bool _isBuilded = false;

        /// <summary>
        /// Registered runtime methods
        /// </summary>
        private readonly HashedMethodContainer _runtimeMethods = new HashedMethodContainer();

        /// <summary>
        /// Registered direct types
        /// </summary>
        private readonly Dictionary<Type, RuntimeTypeDefinition> _directTypes = new Dictionary<Type, RuntimeTypeDefinition>();

        private readonly HashSet<string> _directSignatures = new HashSet<string>();

        private readonly List<Edit> _staticEdits = new List<Edit>();

        /// <summary>
        /// Registered data types
        /// </summary>
        private readonly Dictionary<string, DataTypeDefinition> _dataTypes = new Dictionary<string, DataTypeDefinition>();

        private readonly Dictionary<string, GeneratorProvider> _methodGeneratorProviders;

        private readonly Dictionary<string, InheritanceChain> _inheritanceChains = new Dictionary<string, InheritanceChain>();

        private readonly string _fullPath = "//Runtime";

        protected override string getAssemblyFullPath()
        {
            return _fullPath;
        }

        protected override string getAssemblyName()
        {
            return "Runtime";
        }

        public RuntimeAssembly(string fullPath = null)
        {
            if (fullPath != null)
                _fullPath = fullPath;

            _methodGeneratorProviders = new Dictionary<string, GeneratorProvider>()
            {
                {"_method_",_createMethod},
                {"_get_",_createProperty},
                {"_set_",_createProperty},
            };

            var chain = new InheritanceChain(TypeDescriptor.Create<object>(), new InheritanceChain[0]);
            _inheritanceChains.Add(chain.Path.Signature, chain);
            //TODO refactor array support
            var arrayDefinition = new DirectTypeDefinition<Array<InstanceWrap>>();
            arrayDefinition.ForcedInfo = ArrayInfo;
            arrayDefinition.ForcedSubTypes = new[]{
                typeof(IEnumerable<>),
                typeof(System.Collections.IEnumerable),
            };

            AddDirectDefinition(arrayDefinition);

            /*   var objDefinition = new DirectTypeDefinition<ObjectDefinition>();
               objDefinition.ForcedInfo = ObjectInfo;
               AddDirectDefinition(objDefinition);*/
        }

        /// <summary>
        /// Add runtime type definition into runtime assembly
        /// </summary>
        /// <param name="definition">Added type definition</param>
        public void AddDefinition(DataTypeDefinition definition)
        {
            _dataTypes.Add(definition.FullName, definition);
        }


        public void AddDirectDefinition(DirectTypeDefinition definition)
        {
            _directTypes[definition.DirectType] = definition;

            var typeInfo = definition.TypeInfo;
            _directSignatures.Add(new PathInfo(typeInfo.TypeName).Signature);
        }

        public void BuildAssembly()
        {
            if (_isBuilded)
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
                if (dataType.ComponentInfo != null)
                {
                    AddComponent(dataType.ComponentInfo);
                }
            }
            _isBuilded = true;
        }

        internal InheritanceChain GetChain(Type type)
        {
            //TODO resolve generic chains
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


        internal bool IsInDirectCover(Type type)
        {
            return
                type.IsGenericParameter ||
                type == typeof(void) ||
                type.IsArray ||
                typeof(Instance).IsAssignableFrom(type) ||
                type == typeof(InstanceWrap) ||
                _directTypes.ContainsKey(type);
        }

        internal bool IsDirectType(InstanceInfo typeInfo)
        {
            var signature = new PathInfo(typeInfo.TypeName).Signature;
            return _directSignatures.Contains(signature);
        }

        #region Assembly provider implementation

        public override SearchIterator CreateRootIterator()
        {
            return new HashIterator(_runtimeMethods);
        }

        public override GeneratorBase GetGenericMethodGenerator(MethodID method, PathInfo searchPath)
        {
            return _runtimeMethods.AccordingGenericId(method, searchPath);
        }

        public override GeneratorBase GetMethodGenerator(MethodID method)
        {
            return _runtimeMethods.AccordingId(method);
        }

        public override MethodID GetImplementation(MethodID method, TypeDescriptor dynamicInfo)
        {
            return _runtimeMethods.GetImplementation(method, dynamicInfo);
        }

        public override MethodID GetGenericImplementation(MethodID methodID, PathInfo methodSearchPath, PathInfo implementingTypePath)
        {
            return _runtimeMethods.GetGenericImplementation(methodID, methodSearchPath, implementingTypePath);
        }

        public override InheritanceChain GetInheritanceChain(PathInfo typePath)
        {
            InheritanceChain chain;
            _inheritanceChains.TryGetValue(typePath.Signature, out chain);


            if (chain.Type.HasParameters)
            {
                chain = chain.MakeGeneric(typePath.GenericArgs);
            }

            return chain;
        }

        #endregion

        #region TypeDefinition building

        /// <summary>
        /// Register given definition into runtime assembly
        /// </summary>
        /// <param name="definition">Registered definition</param>
        private void buildDefinition(RuntimeTypeDefinition definition)
        {
            //every definition needs initialization
            definition.Initialize(this, this.TypeServices);

            //add static edits defined by builded definition
            _staticEdits.AddRange(definition.StaticEdits);

            //efery definition needs to register its chain
            registerChain(definition);

            //get all methods defined by definition
            var methodGenerators = definition.GetMethods();
            foreach (var generator in methodGenerators)
            {
                var item = new MethodItem(generator, generator.MethodInfo);
                _runtimeMethods.AddItem(item, generator.Implemented);
            }
        }

        /// <summary>
        /// Register inheritance chain for given definition
        /// </summary>
        /// <param name="definition">Definition which chain is registered</param>
        private void registerChain(RuntimeTypeDefinition definition)
        {
            var subChains = definition.GetSubChains();

            var chain = new InheritanceChain(definition.TypeInfo, subChains);
            _inheritanceChains[chain.Path.Signature] = chain;
        }

        /// <summary>
        /// Get method generators defined in given runtime type definition
        /// </summary>
        /// <param name="definition">Definition which methods are resolved</param>
        /// <returns>Method generators for defined methods</returns>
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
        /// Build method generator from given method info
        /// </summary>
        /// <param name="definition">Type where method is defined</param>
        /// <param name="method">Method info defining method</param>
        /// <param name="methodName">Name of defined method</param>
        /// <returns>Builder where method is builded</returns>
        private RuntimeMethodGenerator buildMethod(RuntimeTypeDefinition definition, MethodInfo method, string methodName)
        {

            var builder = new MethodBuilder(definition, methodName);
            builder.ThisObjectExpression = builder.DeclaringDefinitionConstant;
            builder.AdapterFor(method);
            return builder.Build();
        }

        /// <summary>
        /// Determine, that given parameter needs wrapping
        /// </summary>
        /// <param name="par">Parameter which wrapping is determined</param>
        /// <returns>True if parameter needs wrapping, false otherwise</returns>
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

        private RuntimeMethodGenerator _createMethod(RuntimeTypeDefinition definition, MethodInfo method, string name)
        {
            if (name == "ctor")
            {
                name = "#ctor";
            }
            var generator = buildMethod(definition, method, name);

            return generator;
        }

        private RuntimeMethodGenerator _createProperty(RuntimeTypeDefinition definition, MethodInfo method, string name)
        {
            var generator = buildMethod(definition, method, method.Name.Substring(1));

            return generator;
        }

        #endregion

        public DataTypeDefinition GetDrawer(Instance instance)
        {
            DataTypeDefinition typeDefinition;
            _dataTypes.TryGetValue(instance.Info.TypeName, out typeDefinition);
            //TODO resolve generic types and inheritance
            return typeDefinition;
        }

        internal RuntimeTypeDefinition GetTypeDefinition(Instance instance)
        {
            //TODO: what about direct types ?
            DataTypeDefinition typeDefinition;
            _dataTypes.TryGetValue(instance.Info.TypeName, out typeDefinition);

            return typeDefinition;
        }

        public DrawingPipeline CreateDrawingPipeline(GeneralDrawer drawer, AnalyzingResult result)
        {
            return new DrawingPipeline(drawer, this, result);
        }
    }
}
