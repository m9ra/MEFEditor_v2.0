using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

namespace TypeSystem
{
    public class InheritanceChain
    {
        public readonly PathInfo Path;

        public readonly TypeDescriptor Type;

        public readonly IEnumerable<InheritanceChain> SubChains;

        internal InheritanceChain(TypeDescriptor type, IEnumerable<InheritanceChain> subChains)
        {
            Type = type;
            Path = new PathInfo(type.TypeName);
            var filtered = from subChain in subChains where subChain != null select subChain;
            SubChains = filtered.ToArray();
        }

        public bool HasSubChain(string targetTypeName)
        {
            if (targetTypeName == Type.TypeName)
            {
                return true;
            }

            foreach (var subChain in SubChains)
            {
                var hasSubType = subChain.HasSubChain(targetTypeName);
                if (hasSubType)
                    return true;
            }

            return false;
        }

        internal InheritanceChain MakeGeneric(IEnumerable<string> arguments)
        {
            var substitutions = new Dictionary<string, string>();

            var args = arguments.ToArray();
            var pars = Type.Arguments.ToArray();
            for (var i = 0; i < args.Length; ++i)
            {
                var par = pars[i];
                var arg = args[i];
                if (!par.IsParameter)
                    continue;

                substitutions[par.TypeName] = arg;
            }

            if (substitutions.Count == 0)
                return this;

            var genericType = Type.MakeGeneric(substitutions);
            var genericSubChains=new List<InheritanceChain>();

            foreach (var subChain in SubChains)
            {
                var childArguments = new List<string>();
                foreach (var param in subChain.Type.Arguments)
                {
                    string substitution;
                    if (substitutions.TryGetValue(param.TypeName, out substitution))
                    {
                        //substitution is found
                        childArguments.Add(substitution);
                    }
                    else
                    {
                        //keep old parameter as is
                        childArguments.Add(param.TypeName);
                    }
                }

                var genericSubChain = subChain.MakeGeneric(childArguments);
                genericSubChains.Add(genericSubChain);
            }

            return new InheritanceChain(genericType, genericSubChains);
        }
    }
}
