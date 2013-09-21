using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using TypeSystem;

namespace AssemblyProviders.CSharp.Compiling
{
    class Context
    {
        public readonly EmitterBase Emitter;
        public readonly TypeServices Services;
        private readonly Dictionary<string, string> _genericMapping = new Dictionary<string, string>();
        private static readonly Dictionary<Type, string> Aliases = new Dictionary<Type, string>();
        private static readonly Dictionary<string, string> TypeNameLookup = new Dictionary<string, string>();

        static Context()
        {
            AddAlias<byte>("byte");
            AddAlias<sbyte>("sbyte");
            AddAlias<short>("short");
            AddAlias<ushort>("ushort");
            AddAlias<int>("int");
            AddAlias<uint>("uint");
            AddAlias<long>("long");
            AddAlias<ulong>("ulong");
            AddAlias<float>("float");
            AddAlias<double>("double");
            AddAlias<decimal>("decimal");
            AddAlias<object>("object");
            AddAlias<bool>("bool");
            AddAlias<char>("char");
            AddAlias<string>("string");
        }

        private static void AddAlias<T>(string alias)
        {
            var type=typeof(T);
            Aliases[type] = alias;
            TypeNameLookup[alias] = type.FullName;
        }

        public Context(EmitterBase emitter, TypeServices services)
        {
            if (emitter == null)
                throw new ArgumentNullException("emitter");

            if (services == null)
                throw new ArgumentNullException("services");

            Emitter = emitter;
            Services = services;
        }


        public MethodSearcher CreateSearcher()
        {
            return Services.CreateSearcher();
        }

        internal void SetTypeMapping(string genericParam, string genericArg)
        {
            _genericMapping[genericParam] = genericArg;
        }

        internal string Map(string name)
        {
            string result;

            if(TypeNameLookup.TryGetValue(name, out result)){
                return result;
            }

            if (_genericMapping.TryGetValue(name, out result))
            {
                //mapping has been found
                return result;
            }

            //there is no mapping
            return name;
        }
    }
}
