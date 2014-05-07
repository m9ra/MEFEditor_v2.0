using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using TypeSystem;

using AssemblyProviders.CSharp.Interfaces;
using AssemblyProviders.CSharp.Transformations;

using TypeSystem.Runtime;

namespace AssemblyProviders.CSharp.Compiling
{

    /// <summary>
    /// Base class for providing emitting both lvalues and rvalues
    /// </summary>
    abstract class ValueProvider
    {
        /// <summary>
        /// Context of compilation process
        /// </summary>
        protected readonly CompilationContext Context;

        /// <summary>
        /// Emiter that can be used within generation methods
        /// </summary>
        protected EmitterBase E { get { return Context.Emitter; } }

        protected ValueProvider(CompilationContext context)
        {
            Context = context;
        }
    }

    /// <summary>
    /// Base class for providing rvalue emitting.
    /// </summary>
    abstract class RValueProvider : ValueProvider
    {
        /// <summary>
        /// Generate instructions that ensure presence of represented
        /// rvalue in given target
        /// </summary>
        /// <param name="target">Target where represented value will be available</param>
        public abstract void GenerateAssignInto(LValueProvider target);

        /// <summary>
        /// Generate return statement passing represented value
        /// </summary>
        public abstract void GenerateReturn();

        /// <summary>
        /// Generate instructions needed for rvalue computation. No
        /// assigned storage is needed
        /// </summary>
        public abstract void Generate();

        /// <summary>
        /// Get type information about rvalue.
        /// <remarks>May cause internal calling of generation methods.</remarks>
        /// </summary>
        /// <returns>Type informtion about rvalue</returns>
        public abstract TypeDescriptor Type { get; }

        /// <summary>
        /// Get storage where RValue is available. If there is no such storage, temporary variable is used
        /// <remarks>May cause internal calling of generation methods.</remarks>
        /// </summary>
        /// <returns>Name of storage variable</returns>
        public abstract string GenerateStorage();


        protected RValueProvider(CompilationContext context) : base(context) { }

    }



    class NewObjectValue : RValueProvider
    {
        readonly TypeDescriptor _objectType;

        RValueProvider _ctorCall;
        string _storage;

        public NewObjectValue(TypeDescriptor objectType, CompilationContext context) :
            base(context)
        {
            _objectType = objectType;
        }

        /// </ inheritdoc>
        public override void GenerateAssignInto(LValueProvider lValue)
        {
            var storageProvider = lValue as IStorageReadProvider;
            if (storageProvider == null)
            {
                throw new NotImplementedException("Create temporary variable and reasign");
            }
            else
            {
                _storage = storageProvider.Storage;
            }

            lValue.AssignNewObject(_objectType);

            _ctorCall.Generate();
        }

        /// </ inheritdoc>
        public override void GenerateReturn()
        {
            throw new NotImplementedException();
        }

        /// </ inheritdoc>
        public override TypeDescriptor Type
        {
            get
            {
                return _objectType;
            }
        }

        /// </ inheritdoc>
        public override string GenerateStorage()
        {
            if (_storage == null)
            {
                throw new NotSupportedException("Object hasn't been created yet");
            }
            return _storage;
        }

        /// </ inheritdoc>
        public override void Generate()
        {
            throw new NotImplementedException();
        }

        internal void SetCtor(RValueProvider ctorCall)
        {
            _ctorCall = ctorCall;
        }
    }

    class LiteralValue : RValueProvider
    {
        private readonly object _literal;
        private readonly TypeDescriptor _literalInfo;
        private readonly INodeAST _literalNode;
        public LiteralValue(object literal, INodeAST literalNode, CompilationContext context)
            : base(context)
        {
            _literal = literal;
            _literalNode = literalNode;
            _literalInfo = TypeDescriptor.Create(_literal.GetType());
        }

        /// </ inheritdoc>
        public override void GenerateAssignInto(LValueProvider lValue)
        {
            lValue.AssignLiteral(_literal, _literalNode);
        }

        /// </ inheritdoc>
        public override void GenerateReturn()
        {
            E.Return(GenerateStorage());
        }

        /// </ inheritdoc>
        public override TypeDescriptor Type
        {
            get
            {
                return _literalInfo;
            }
        }

        /// </ inheritdoc>
        public override string GenerateStorage()
        {
            var temporaryName = E.GetTemporaryVariable();
            E.AssignLiteral(temporaryName, _literal);
            return temporaryName;
        }

        /// </ inheritdoc>
        public override void Generate()
        {
            //Nothing to generate
        }
    }

    class VariableRValue : RValueProvider
    {
        private readonly VariableInfo _variable;
        private readonly INodeAST _variableNode;

        public VariableRValue(VariableInfo variable, INodeAST variableNode, CompilationContext context)
            : base(context)
        {
            variable.AddVariableUsing(variableNode);
            _variable = variable;
            _variableNode = variableNode;
        }

        /// </ inheritdoc>
        public override void GenerateAssignInto(LValueProvider lValue)
        {
            lValue.Assign(_variable.Name, _variableNode);
        }

        /// </ inheritdoc>
        public override void GenerateReturn()
        {
            E.Return(_variable.Name);
        }

        /// </ inheritdoc>
        public override TypeDescriptor Type
        {
            get
            {
                return E.VariableInfo(_variable.Name) as TypeDescriptor;
            }
        }

        /// </ inheritdoc>
        public override string GenerateStorage()
        {
            return _variable.Name;
        }

        /// </ inheritdoc>
        public override void Generate()
        {
            //Nothing to generate
        }
    }

    class DefaultArgValue : RValueProvider
    {
        private readonly object _defaultValue;
        private readonly TypeDescriptor _resultType;

        internal DefaultArgValue(object defaultValue, TypeDescriptor resultType, CompilationContext context)
            : base(context)
        {
            _defaultValue = defaultValue;
            _resultType = resultType;
        }

        /// </ inheritdoc>
        public override void GenerateAssignInto(LValueProvider lValue)
        {
            throw new NotImplementedException();
        }

        /// </ inheritdoc>
        public override void GenerateReturn()
        {
            throw new NotImplementedException();
        }

        /// </ inheritdoc>
        public override TypeDescriptor Type
        {
            get
            {
                return _resultType;
            }
        }

        /// </ inheritdoc>
        public override string GenerateStorage()
        {
            var tmp = E.GetTemporaryVariable("default");
            E.AssignLiteral(tmp, _defaultValue, _resultType);
            return tmp;
        }

        /// </ inheritdoc>
        public override void Generate()
        {
            throw new NotImplementedException();
        }
    }

    class ParamArgValue : RValueProvider
    {
        private readonly TypeDescriptor _arrayType;
        private readonly RValueProvider[] _args;

        public ParamArgValue(TypeDescriptor arrayType, RValueProvider[] args, CompilationContext context)
            : base(context)
        {
            _arrayType = arrayType;
            _args = args;
        }

        /// </ inheritdoc>
        public override void GenerateAssignInto(LValueProvider lValue)
        {
            throw new NotImplementedException();
        }

        /// </ inheritdoc>
        public override void GenerateReturn()
        {
            throw new NotImplementedException();
        }

        /// </ inheritdoc>
        public override TypeDescriptor Type
        {
            get
            {
                return _arrayType;
            }
        }

        /// </ inheritdoc>
        public override string GenerateStorage()
        {
            var tmp = E.GetTemporaryVariable("param");
            var array = new Array<InstanceWrap>(_args.Length);
            E.AssignLiteral(tmp, array);

            var setMethod = array.SetItemMethod;
            var index = E.GetTemporaryVariable("index");
            for (int i = 0; i < _args.Length; ++i)
            {
                var arg = _args[i];
                E.AssignLiteral(index, i);

                E.Call(setMethod, tmp, Arguments.Values(index, arg.GenerateStorage()));
            }
            return tmp;
        }

        /// </ inheritdoc>
        public override void Generate()
        {
            throw new NotImplementedException();
        }
    }

    class CallValue : RValueProvider
    {
        private readonly CallActivation _activation;

        internal TypeMethodInfo MethodInfo { get { return _activation.MethodInfo; } }

        public CallValue(CallActivation activation, CompilationContext context)
            : base(context)
        {
            if (activation == null)
                throw new ArgumentNullException("activation");

            _activation = activation;
        }

        /// </ inheritdoc>
        public override void GenerateAssignInto(LValueProvider lValue)
        {
            generateCall();
            lValue.AssignReturnValue(MethodInfo.ReturnType);
        }

        /// </ inheritdoc>
        public override void GenerateReturn()
        {
            generateCall();
            var temporaryName = E.GetTemporaryVariable();
            E.AssignReturnValue(temporaryName, MethodInfo.ReturnType);
            E.Return(temporaryName);
        }

        private void generateCall()
        {
            var argVariables = new List<string>();
            foreach (var arg in _activation.Arguments)
            {
                argVariables.Add(arg.GenerateStorage());
            }

            var args = Arguments.Values(argVariables.ToArray());

            if (_activation.MethodInfo.IsStatic)
            {
                E.StaticCall(MethodInfo.DeclaringType, MethodInfo.MethodID, args);
            }
            else
            {
                var objStorage = _activation.CalledObject.GenerateStorage();

                var builder = E.Call(MethodInfo.MethodID, objStorage, args);
                builder.SetTransformationProvider(new CallProvider(_activation.CallNode));
            }
        }

        /// </ inheritdoc>
        public override TypeDescriptor Type
        {
            get
            {
                return MethodInfo.ReturnType;
            }
        }

        /// </ inheritdoc>
        public override string GenerateStorage()
        {
            var tmp = new TemporaryVariableValue(MethodInfo.ReturnType, Context);
            GenerateAssignInto(tmp);

            return tmp.Storage;
        }

        /// </ inheritdoc>
        public override void Generate()
        {
            generateCall();
        }
    }

    /// <summary>
    /// Emit computation and assign computed result into given storage
    /// </summary>
    /// <param name="emitter">Emitter where instructions has to be emitted</param>
    /// <param name="storage">Storage where computed value has to be assigned</param>
    delegate void EmitComputation(EmitterBase emitter, LValueProvider storage);

    class ComputedValue : RValueProvider
    {
        private readonly EmitComputation _computation;

        private readonly TypeDescriptor _type;

        private LValueProvider _storage;

        private bool _generated;

        public ComputedValue(TypeDescriptor type, EmitComputation computation, CompilationContext context)
            : base(context)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            _type = type;
            _computation = computation;
        }

        #region Value provider implementation

        /// </ inheritdoc>
        public override void GenerateAssignInto(LValueProvider lValue)
        {
            generateAssignToStorage(lValue);
        }

        /// </ inheritdoc>
        public override void GenerateReturn()
        {
            var storage = getStorage();
            E.Return(storage);
        }

        /// </ inheritdoc>
        public override TypeDescriptor Type
        {
            get
            {
                return _type;
            }
        }

        /// </ inheritdoc>
        public override string GenerateStorage()
        {
            return getStorage();
        }

        /// </ inheritdoc>
        public override void Generate()
        {
            //storage is not needed
            generateAssignToStorage(null);
        }

        #endregion

        #region Computation utilities

        /// <summary>
        /// Get storage with computed result. Consider current state of computation.
        /// </summary>
        /// <returns>Storage where computation is stored</returns>
        private string getStorage()
        {
            if (_storage == null)
            {
                var tmp = new TemporaryVariableValue(_type, Context);
                generateAssignToStorage(tmp);
            }

            var storageProvider = _storage as IStorageReadProvider;
            return storageProvider.Storage;
        }

        /// <summary>
        /// Generate computation or assigning that are needed
        /// </summary>
        /// <param name="target">Target where value will be assigned to</param>
        private void generateAssignToStorage(LValueProvider target)
        {
            if (_generated && _storage == target)
            {
                //storage is already assigned to requested
                // varialbe. Also computation is generated.
                return;
            }

            if (_storage != null)
            {
                //reasign value

                throw new NotImplementedException();
            }

            //remember, target can be null
            _storage = target;

            if (_generated)
                return;

            _generated = true;

            _computation(E, _storage);
            //  throw new NotImplementedException();
        }

        #endregion
    }

    class TemporaryRVariableValue : RValueProvider
    {
        public readonly string Storage;

        public TemporaryRVariableValue(CompilationContext context, string storage = null)
            : base(context)
        {
            if (storage == null)
            {
                Storage = E.GetTemporaryVariable();
            }
            else
            {
                Storage = storage;
            }

        }

        /// </ inheritdoc>
        public override void GenerateAssignInto(LValueProvider lValue)
        {
            lValue.Assign(Storage, null);
        }

        /// </ inheritdoc>
        public override void GenerateReturn()
        {
            E.Return(Storage);
        }

        /// </ inheritdoc>
        public override TypeDescriptor Type
        {
            get
            {
                return E.VariableInfo(Storage) as TypeDescriptor;
            }
        }

        /// </ inheritdoc>
        public override string GenerateStorage()
        {
            return Storage;
        }

        /// </ inheritdoc>
        public override void Generate()
        {
            throw new NotImplementedException();
        }
    }
}
