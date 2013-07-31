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
        public readonly IEmitter<MethodID, InstanceInfo> Emitter;
        public readonly TypeServices Services;

        public Context(IEmitter<MethodID, InstanceInfo> emitter,TypeServices services)
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
    }
}
