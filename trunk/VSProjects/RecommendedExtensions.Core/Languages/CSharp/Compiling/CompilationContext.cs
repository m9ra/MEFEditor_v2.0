using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;
using MEFEditor.TypeSystem;

using RecommendedExtensions.Core.Languages.CSharp.Interfaces;

namespace RecommendedExtensions.Core.Languages.CSharp.Compiling
{
    /// <summary>
    /// Context which encapsulates services provided by <see cref="Compiler" /> to <see cref="ValueProvider" /> implementations.
    /// </summary>
    class CompilationContext
    {
        /// <summary>
        /// Mapping of generic parameters to their fullnames.
        /// </summary>
        private readonly Dictionary<string, string> _genericMapping = new Dictionary<string, string>();

        /// <summary>
        /// Mapping of types to their aliases.
        /// </summary>
        private static readonly Dictionary<Type, string> Aliases = new Dictionary<Type, string>();

        /// <summary>
        /// Mapping of aliases to their type names.
        /// </summary>
        internal static readonly Dictionary<string, string> AliasLookup = new Dictionary<string, string>();

        /// <summary>
        /// Stack of block contexts.
        /// </summary>
        private readonly Stack<BlockContext> _blockContexts = new Stack<BlockContext>();

        /// <summary>
        /// Emitter exposed by context. It is used for emitting instructions from <see cref="ValueProvider" /> implementations.
        /// </summary>
        public readonly EmitterBase Emitter;

        /// <summary>
        /// Services provided by type system.
        /// </summary>
        public readonly TypeServices Services;

        /// <summary>
        /// Soruce of compiled method.
        /// </summary>
        public readonly Source Source;

        /// <summary>
        /// Context of current active block.
        /// </summary>
        /// <value>The current block.</value>
        public BlockContext CurrentBlock
        {
            get
            {
                if (_blockContexts.Count == 0)
                    return null;

                return _blockContexts.Peek();
            }
        }

        /// <summary>
        /// Initialize alias tables.
        /// </summary>
        static CompilationContext()
        {
            RegisterAlias<byte>("byte");
            RegisterAlias<sbyte>("sbyte");
            RegisterAlias<short>("short");
            RegisterAlias<ushort>("ushort");
            RegisterAlias<int>("int");
            RegisterAlias<uint>("uint");
            RegisterAlias<long>("long");
            RegisterAlias<ulong>("ulong");
            RegisterAlias<float>("float");
            RegisterAlias<double>("double");
            RegisterAlias<decimal>("decimal");
            RegisterAlias<object>("object");
            RegisterAlias<bool>("bool");
            RegisterAlias<char>("char");
            RegisterAlias<string>("string");
        }

        /// <summary>
        /// Register T with given alias.
        /// </summary>
        /// <typeparam name="T">Registered type.</typeparam>
        /// <param name="alias">Alias for registered type.</param>
        private static void RegisterAlias<T>(string alias)
        {
            var type = typeof(T);
            Aliases[type] = alias;
            AliasLookup[alias] = type.FullName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompilationContext"/> class.
        /// </summary>
        /// <param name="emitter">The emitter.</param>
        /// <param name="source">The source.</param>
        /// <param name="services">The services.</param>
        /// <exception cref="System.ArgumentNullException">
        /// emitter
        /// or
        /// source
        /// or
        /// services
        /// </exception>
        internal CompilationContext(EmitterBase emitter, Source source, TypeServices services)
        {
            if (emitter == null)
                throw new ArgumentNullException("emitter");

            if (source == null)
                throw new ArgumentNullException("source");

            if (services == null)
                throw new ArgumentNullException("services");

            Emitter = emitter;
            Source = source;
            Services = services;
        }

        /// <summary>
        /// Create searcher provided byt TypeSystem. Searcher provides ability to search method
        /// according to its name in all assemblies reachable for defining assembly.
        /// </summary>
        /// <returns>Created <see cref="MethodSearcher" />.</returns>
        public MethodSearcher CreateSearcher()
        {
            return Services.CreateSearcher();
        }

        /// <summary>
        /// Register argument for given parameter.
        /// </summary>
        /// <param name="parameter">Generic parameter which argument is registered.</param>
        /// <param name="argument">Argument available for given generic parameter.</param>
        internal void RegisterGenericArgument(string parameter, string argument)
        {
            _genericMapping[parameter] = argument;
        }

        /// <summary>
        /// Map given path according to aliases and generic arguments
        /// All generic arguments are expanded to fullname form.
        /// </summary>
        /// <param name="path">Path that should be mapped.</param>
        /// <returns>Fullname of mapped type if mapping is available, or unchanged type name otherwise.</returns>
        internal string MapGeneric(string path)
        {
            //whole name mapping
            string result;
            if (
                //mapped name belongs to alias
                AliasLookup.TryGetValue(path, out result) ||
                //mapping has been found between generic arguments
                _genericMapping.TryGetValue(path, out result)
                )
            {
                //in substitutions there are parameters already translated
                return result;
            }

            //use namespace expansion only for arguments
            return TypeDescriptor.TranslatePath(path, pathSubstitutions);
        }

        /// <summary>
        /// Get <see cref="TypeDescriptor" /> from mapped typeNameSuffix by
        /// namespace expansion.
        /// </summary>
        /// <param name="typeNameSuffix">Mapped suffix of searched type.</param>
        /// <returns><see cref="TypeDescriptor" /> from expanded type name suffix if type is available, <c>null</c> otherwise.</returns>
        internal TypeDescriptor DescriptorFromSuffix(string typeNameSuffix)
        {
            foreach (var ns in Source.Namespaces)
            {
                var fullname = ns == "" ? typeNameSuffix : ns + "." + typeNameSuffix;
                var typeDescriptor = TypeDescriptor.Create(fullname);
                var chain = Services.GetChain(typeDescriptor);
                if (chain != null)
                    return typeDescriptor;
            }
            return null;
        }

        /// <summary>
        /// Push context of given block.
        /// </summary>
        /// <param name="block">Block which context is pushed.</param>
        /// <param name="continueLabel">Label that will be used for continue statement within block.</param>
        /// <param name="breakLabel">Label that will be used for break statement within block.</param>
        internal void PushBlock(INodeAST block, Label continueLabel, Label breakLabel)
        {
            var context = new BlockContext(block, continueLabel, breakLabel);
            _blockContexts.Push(context);
        }

        /// <summary>
        /// Pop context of peek block.
        /// </summary>
        internal void PopBlock()
        {
            _blockContexts.Pop();
        }

        /// <summary>
        /// Substitute given parameter according current aliases, generic mappings and namespaces.
        /// </summary>
        /// <param name="pathParameter">Parameter to be translated.</param>
        /// <returns>Translated parameter.</returns>
        private string pathSubstitutions(string pathParameter)
        {
            //whole parameter mapping
            string substitution;
            if (
                //mapped name belongs to alias
                AliasLookup.TryGetValue(pathParameter, out substitution) ||
                //mapping has been found between generic arguments
                _genericMapping.TryGetValue(pathParameter, out substitution)
                )
                return substitution;

            //try to expand parameter
            var suffixed = DescriptorFromSuffix(pathParameter);
            if (suffixed != null)
                //pathParameter is type suffix and can be expanded
                //by available namespaces
                return suffixed.TypeName;

                
            return pathParameter;
        }
    }
}
