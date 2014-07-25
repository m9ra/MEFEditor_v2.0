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

        internal readonly bool IsIndexer;

        internal MethodSelector(IEnumerable<TypeMethodInfo> overloads, CompilationContext context)
        {
            foreach (var overload in overloads)
            {
                _argumentIterators.AddFirst(new ArgumentIterator(overload, context));

                var name = overload.MethodName;
                IsIndexer |= name == Naming.IndexerSetter || name == Naming.IndexerGetter;
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
        private readonly Dictionary<string, ParameterTypeInfo> _unresolvedParameters = new Dictionary<string, ParameterTypeInfo>();

        private readonly MultiDictionary<ParameterTypeInfo, RValueProvider> _argBindings = new MultiDictionary<ParameterTypeInfo, RValueProvider>();
        private readonly MultiDictionary<ParameterTypeInfo, TypeDescriptor> _genericBindings = new MultiDictionary<ParameterTypeInfo, TypeDescriptor>();

        private readonly TypeMethodInfo _overload;

        private readonly CompilationContext _context;

        /// <summary>
        /// Index of ordered argument to accept
        /// </summary>
        private int _orderedArgIndex;

        internal bool IsValid { get; private set; }

        internal ArgumentIterator(TypeMethodInfo overload, CompilationContext context)
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

        private ParameterTypeInfo getParamToMatch(Argument argument)
        {
            ParameterTypeInfo paramToMatch = null;
            if (argument.IsNamed)
            {
                _unresolvedParameters.TryGetValue(argument.Name, out paramToMatch);
            }
            else
            {
                paramToMatch = getCurrentParam();
                if (paramToMatch != null && !paramToMatch.HasParam)
                    //shift while we have HasParam parameter
                    //we can accept multiple arguments
                    ++_orderedArgIndex;
            }
            return paramToMatch;
        }

        private ParameterTypeInfo getCurrentParam()
        {
            if (_orderedArgIndex >= _overload.Parameters.Length)
                return null;

            return _overload.Parameters[_orderedArgIndex];
        }

        private void bind(ParameterTypeInfo param, Argument arg)
        {
            //TODO resolve score...
            if (param.Type.IsParameter)
            {
                _genericBindings.Add(param, arg.Value.Type);
            }
            else
            {
                var type = arg.Value.Type;
                var isNull = type != null && type.TypeName == typeof(DirectDefinitions.NullLiteral).FullName;
                if (!isNull && !param.HasParam && !_context.Services.IsAssignable(param.Type, type))
                {
                    //type mismatch
                    IsValid = false;
                    return;
                }
            }

            _argBindings.Add(param, arg.Value);
        }

        internal CallActivation CreateCallActivation()
        {
            var overload = resolveGenericBinding(_overload);
            var activation = new CallActivation(overload);

            foreach (var param in overload.Parameters)
            {
                RValueProvider arg;

                if (isUnresolved(param))
                {
                    if (param.HasDefaultValue)
                    {
                        //there is default value for the argument
                        arg = new DefaultArgValue(param.DefaultValue, param.Type, _context);
                    }
                    else if (param.HasParam)
                    {
                        arg = new ParamArgValue(param.Type, new RValueProvider[0], _context);
                    }
                    else
                    {
                        //parameter doesnt match
                        return null;
                    }
                }
                else
                {
                    var args = _argBindings.Get(param).ToArray();

                    if (param.HasParam)
                    {
                        arg = new ParamArgValue(param.Type, args, _context);
                    }
                    else if (args.Length != 1)
                    {
                        throw new NotSupportedException("Wrong argument count for parameter: " + param.Name);
                    }
                    else
                    {
                        //TODO type conversions
                        arg = args[0];
                    }
                }

                activation.AddArgument(arg);
            }


            return activation;
        }

        private TypeMethodInfo resolveGenericBinding(TypeMethodInfo methodDefinition)
        {
            if (!_genericBindings.Keys.Any())
            {
                //there are no bindings available
                return methodDefinition;
            }

            var translations = new Dictionary<string, string>();

            foreach (var param in _genericBindings.Keys)
            {
                var translatedName = param.Type.TypeName;
                if (translations.ContainsKey(translatedName))
                    //we already have translation
                    continue;

                var bindings = _genericBindings.Get(param).ToArray();
                if (bindings.Length > 1)
                {
                    throw new NotImplementedException("Determine binding for parametric parameters");
                }

                translations.Add(translatedName, bindings[0].TypeName);
            }

            //remap argument bindings from definitions parameters to current method parameters
            var genericMethod = methodDefinition.MakeGenericMethod(translations);
            for (var i = 0; i < genericMethod.Parameters.Length; ++i)
            {
                var currentParameter = genericMethod.Parameters[i];
                var oldParameter = methodDefinition.Parameters[i];

                var args = _argBindings.Get(oldParameter);
                _argBindings.Set(currentParameter, args);
            }

            return genericMethod;
        }

        private bool isUnresolved(ParameterTypeInfo param)
        {
            return _unresolvedParameters.ContainsKey(param.Name);
        }
    }
}
