using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using TypeSystem;

namespace AssemblyProviders.CSharp.Compiling
{
    /// <summary>
    /// Context which encapsulates services provided by <see cref="Compiler"/> to <see cref="ValueProviderr"/> implementations
    /// </summary>
    class CompilationContext
    {
        /// <summary>
        /// Mapping of generic parameters to their fullnames
        /// </summary>
        private readonly Dictionary<string, string> _genericMapping = new Dictionary<string, string>();

        /// <summary>
        /// Mapping of types to their aliases
        /// </summary>
        private static readonly Dictionary<Type, string> Aliases = new Dictionary<Type, string>();

        /// <summary>
        /// Mapping of 
        /// </summary>
        private static readonly Dictionary<string, string> AliasLookup = new Dictionary<string, string>();

        /// <summary>
        /// Emitter exposed by context. It is used for emitting instructions from <see cref="ValueProvider"/> implementations
        /// </summary>
        public readonly EmitterBase Emitter;

        /// <summary>
        /// Services provided by type system
        /// </summary>
        public readonly TypeServices Services;

        /// <summary>
        /// Initialize alias tables
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
        /// Register <see cref="T"/> with given alias
        /// </summary>
        /// <typeparam name="T">Registered type</typeparam>
        /// <param name="alias">Alias for registered type</param>
        private static void RegisterAlias<T>(string alias)
        {
            var type=typeof(T);
            Aliases[type] = alias;
            AliasLookup[alias] = type.FullName;
        }

        internal CompilationContext(EmitterBase emitter, TypeServices services)
        {
            if (emitter == null)
                throw new ArgumentNullException("emitter");

            if (services == null)
                throw new ArgumentNullException("services");

            Emitter = emitter;
            Services = services;
        }

        /// <summary>
        /// Create searcher provided byt TypeSystem. Searcher provides ability to search method        
        /// according to its name in all assemblies reachable for defining assembly.
        /// </summary>
        /// <returns>Created <see cref="MethodSearcher"/></returns>
        public MethodSearcher CreateSearcher()
        {
            return Services.CreateSearcher();
        }

        /// <summary>
        /// Register argument for given parameter
        /// </summary>
        /// <param name="parameter">Generic parameter which argument is registered</param>
        /// <param name="argument">Argument available for given generic parameter</param>
        internal void RegisterGenericArgument(string parameter, string argument)
        {
            _genericMapping[parameter] = argument;
        }

        /// <summary>
        /// Map given type name according to aliases and generic arguments
        /// </summary>
        /// <param name="typeName">Name of type that should be mapped</param>
        /// <returns>Fullname of mapped type if mapping is available, or unchanged type name otherwise</returns>
        internal string Map(string typeName)
        {
            string result;

            if(AliasLookup.TryGetValue(typeName, out result)){
                //mapped name belongs to alias
                return result;
            }

            if (_genericMapping.TryGetValue(typeName, out result))
            {
                //mapping has been found between generic arguments
                return result;
            }

            //there is no mapping
            return typeName;
        }
    }
}
