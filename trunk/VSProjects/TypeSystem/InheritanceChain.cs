using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

namespace TypeSystem
{
    /// <summary>
    /// Represents inheritance chain of types - is used for determining type relations
    /// </summary>
    public class InheritanceChain
    {
        /// <summary>
        /// Type to which current chain belongs
        /// </summary>
        public readonly TypeDescriptor Type;

        /// <summary>
        /// Path of Type name
        /// </summary>
        public readonly PathInfo Path;

        /// <summary>
        /// Inheritance chains of sub types of current inheritenc's Type
        /// </summary>
        public readonly IEnumerable<InheritanceChain> SubChains;

        /// <summary>
        /// Create inheritance chain for given type
        /// </summary>
        /// <param name="type">Type which chain is created</param>
        /// <param name="subChains">Chains from subtypes of type</param>
        internal InheritanceChain(TypeDescriptor type, IEnumerable<InheritanceChain> subChains)
        {
            Type = type;
            Path = new PathInfo(type.TypeName);
            var filtered = from subChain in subChains where subChain != null select subChain;
            SubChains = filtered.ToArray();
        }

        /// <summary>
        /// Determine that given targetTypeName is listed in current Type or sub types
        /// </summary>
        /// <param name="targetTypeName">Searched type name</param>
        /// <returns>True if targetTypeName is found, false otherwise</returns>
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

        /// <summary>
        /// Make generic specialization of current chain according to given arguments.
        /// </summary>
        /// <param name="arguments">Substitution arguments</param>
        /// <returns>Created generic specialization</returns>
        public InheritanceChain MakeGeneric(IEnumerable<string> arguments)
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
            var genericSubChains = makeGenericSubchains(substitutions);

            return new InheritanceChain(genericType, genericSubChains);
        }

        /// <summary>
        /// Make generic specialization from current SubChains
        /// </summary>
        /// <param name="substitutions">Substitutions defined by generic arguments and Type parameters</param>
        /// <returns>Generic specialzation</returns>
        private IEnumerable<InheritanceChain> makeGenericSubchains(Dictionary<string, string> substitutions)
        {
            var genericSubChains = new List<InheritanceChain>();

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
            return genericSubChains;
        }

        ///</inheritdoc>
        public override string ToString()
        {
            return string.Format("[Inheritance]{0}", Type.TypeName);
        }
    }
}
