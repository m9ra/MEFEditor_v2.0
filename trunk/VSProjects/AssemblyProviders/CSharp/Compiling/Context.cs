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

        public Context(EmitterBase emitter,TypeServices services)
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
