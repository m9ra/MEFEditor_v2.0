using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeSystem;
using Analyzing;
using Utilities;


using AssemblyProviders.CSharp.Compiling;

namespace AssemblyProviders.CSharp.Compiling
{
    class MethodSelector
    {
        LinkedList<ArgumentIterator> _argumentIterators = new LinkedList<ArgumentIterator>();
        internal MethodSelector(IEnumerable<TypeMethodInfo> overloads,Context context)
        {
            foreach (var overload in overloads)
            {
                _argumentIterators.AddFirst(new ArgumentIterator(overload,context));
            }
        }


        internal CallActivation CreateCallActivation(params Argument[] arguments)
        {
            foreach (var argIt in _argumentIterators)
            {
                foreach (var arg in arguments)
                {
                    if (!argIt.AcceptArgument(arg))
                    {
                        break;
                    }
                }

                if (argIt.IsValid)
                {
                    //TODO select one with lowest score
                    var callActivation = argIt.CreateCallActivation();
                    if (callActivation != null)
                    {
                        return callActivation;
                    }
                }
            }
            return null;
        }
    }

    class Argument
    {
        /// <summary>
        /// Determine that this argument is named
        /// </summary>
        internal bool IsNamed { get { return Name != null; } }
        /// <summary>
        /// Name of named argument, null if there is no name specified
        /// </summary>
        internal readonly string Name;
        /// <summary>
        /// Value of argument
        /// </summary>
        internal readonly RValueProvider Value;

        internal Argument(RValueProvider value)
        {
            Value = value;
            Name = null;
        }
    }

    class ArgumentIterator
    {
        private readonly Dictionary<string, TypeParameterInfo> _unresolvedParameters = new Dictionary<string, TypeParameterInfo>();
        private readonly MultiDictionary<TypeParameterInfo, RValueProvider> _argBindings = new MultiDictionary<TypeParameterInfo, RValueProvider>();

        private readonly TypeMethodInfo _overload;

        private readonly Context _context;

        /// <summary>
        /// Index of ordered argument to accept
        /// </summary>
        private int _orderedArgIndex;

        internal bool IsValid { get; private set; }

        internal ArgumentIterator(TypeMethodInfo overload, Context context)
        {
            _overload = overload;
            _context = context;
            IsValid = true;

            foreach (var param in overload.Parameters)
            {
                _unresolvedParameters.Add(param.Name, param);
            }
        }

        internal bool AcceptArgument(Argument argument)
        {
            var paramToMatch = getParamToMatch(argument);

            if (paramToMatch == null)
            {
                IsValid = false;
            }
            else
            {
                _unresolvedParameters.Remove(paramToMatch.Name);
                bind(paramToMatch, argument);
            }

            return IsValid;
        }

        private TypeParameterInfo getParamToMatch(Argument argument)
        {
            TypeParameterInfo paramToMatch = null;
            if (argument.IsNamed)
            {
                _unresolvedParameters.TryGetValue(argument.Name, out paramToMatch);
            }
            else
            {
                paramToMatch = getCurrentParam();
                ++_orderedArgIndex;
            }
            return paramToMatch;
        }

        private TypeParameterInfo getCurrentParam()
        {
            return _overload.Parameters[_orderedArgIndex];
        }

        private void bind(TypeParameterInfo param, Argument arg)
        {
            //TODO resolve score, inheritance,..
            _argBindings.Add(param, arg.Value);
        }

        internal CallActivation CreateCallActivation()
        {
            var activation = new CallActivation(_overload);

            foreach (var param in _overload.Parameters)
            {
                RValueProvider arg;

                if (isUnresolved(param))
                {
                    if (!param.HasDefaultValue)
                        //missing argument
                        return null;

                    arg = new DefaultArgValue(param.DefaultValue, param.Type, _context);
                }
                else
                {
                    var args = _argBindings.GetExports(param).ToArray();
                    if (args.Length != 1)
                    {
                        throw new NotImplementedException("Resolve params argument");
                    }

                    //TODO type conversions
                    arg = args[0];
                }

                activation.AddArgument(arg);
            }


            return activation;
        }

        private bool isUnresolved(TypeParameterInfo param)
        {
            return _unresolvedParameters.ContainsKey(param.Name);
        }
    }
}
