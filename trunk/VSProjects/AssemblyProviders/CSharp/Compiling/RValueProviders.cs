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

    abstract class ValueProvider
    {
        protected readonly Context Context;

        protected EmitterBase E { get { return Context.Emitter; } }

        protected ValueProvider(Context context)
        {
            Context = context;
        }
    }


    abstract class RValueProvider : ValueProvider
    {
        public abstract void AssignInto(LValueProvider lValue);
        public abstract void Return();
        public abstract InstanceInfo GetResultInfo();
        /// <summary>
        /// Get storage where RValue is available. If there is no such storage, temporary variable is used
        /// </summary>
        /// <returns>Name of storage variable</returns>
        public abstract string GetStorage();

        protected RValueProvider(Context context) : base(context) { }



        public abstract void Generate();
    }




    class NewObjectValue : RValueProvider
    {
        readonly InstanceInfo _objectType;

        RValueProvider _ctorCall;
        string _storage;

        public NewObjectValue(InstanceInfo objectType, Context context) :
            base(context)
        {
            _objectType = objectType;
        }

        public override void AssignInto(LValueProvider lValue)
        {
            _storage = lValue.Storage;
            E.AssignNewObject(_storage, _objectType);
            _ctorCall.Generate();
        }

        public override void Return()
        {
            throw new NotImplementedException();
        }

        public override InstanceInfo GetResultInfo()
        {
            return _objectType;
        }

        public override string GetStorage()
        {
            if (_storage == null)
            {
                throw new NotSupportedException("Object hasn't been created yet");
            }
            return _storage;
        }

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
        private readonly InstanceInfo _literalInfo;
        private readonly INodeAST _literalNode;
        public LiteralValue(object literal, INodeAST literalNode, Context context)
            : base(context)
        {
            _literal = literal;
            _literalNode = literalNode;
            _literalInfo = new InstanceInfo(_literal.GetType());
        }

        public override void AssignInto(LValueProvider lValue)
        {
            var storage = lValue.Storage;
            var builder = E.AssignLiteral(storage, _literal);
            builder.RemoveProvider = new AssignRemove(_literalNode);
        }

        public override void Return()
        {
            E.Return(GetStorage());
        }

        public override InstanceInfo GetResultInfo()
        {
            return _literalInfo;
        }

        public override string GetStorage()
        {
            var temporaryName = E.GetTemporaryVariable();
            E.AssignLiteral(temporaryName, _literal);
            return temporaryName;
        }

        public override void Generate()
        {
            //Nothing to generate
        }
    }

    class VariableRValue : RValueProvider
    {
        private readonly VariableInfo _variable;
        private readonly INodeAST _variableNode;

        public VariableRValue(VariableInfo variable, INodeAST variableNode, Context context)
            : base(context)
        {
            variable.AddVariableUsing(variableNode);
            _variable = variable;
            _variableNode = variableNode;
        }

        public override void AssignInto(LValueProvider lValue)
        {
            var storage = lValue.Storage;
            var builder = E.Assign(storage, _variable.Name);
            builder.RemoveProvider = new AssignRemove(_variableNode);
        }

        public override void Return()
        {
            E.Return(_variable.Name);
        }

        public override InstanceInfo GetResultInfo()
        {
            return E.VariableInfo(_variable.Name);
        }

        public override string GetStorage()
        {
            return _variable.Name;
        }

        public override void Generate()
        {
            //Nothing to generate
        }
    }

    class DefaultArgValue : RValueProvider
    {
        private readonly object _defaultValue;
        private readonly InstanceInfo _resultType;

        internal DefaultArgValue(object defaultValue, InstanceInfo resultType, Context context)
            : base(context)
        {
            _defaultValue = defaultValue;
            _resultType = resultType;
        }

        public override void AssignInto(LValueProvider lValue)
        {
            throw new NotImplementedException();
        }

        public override void Return()
        {
            throw new NotImplementedException();
        }

        public override InstanceInfo GetResultInfo()
        {
            throw new NotImplementedException();
        }

        public override string GetStorage()
        {
            var tmp = E.GetTemporaryVariable("default");
            E.AssignLiteral(tmp, _defaultValue, _resultType);
            return tmp;
        }

        public override void Generate()
        {
            throw new NotImplementedException();
        }
    }

    class ParamArgValue : RValueProvider
    {
        private readonly InstanceInfo _arrayType;
        private readonly RValueProvider[] _args;

        public ParamArgValue(InstanceInfo arrayType, RValueProvider[] args, Context context)
            : base(context)
        {
            _arrayType = arrayType;
            _args = args;
        }

        public override void AssignInto(LValueProvider lValue)
        {
            throw new NotImplementedException();
        }

        public override void Return()
        {
            throw new NotImplementedException();
        }

        public override InstanceInfo GetResultInfo()
        {
            throw new NotImplementedException();
        }

        public override string GetStorage()
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

                E.Call(setMethod, tmp, Arguments.Values(index, arg.GetStorage()));
            }
            return tmp;
        }

        public override void Generate()
        {
            throw new NotImplementedException();
        }
    }

    class CallRValue : RValueProvider
    {
        private readonly CallActivation _activation;

        internal TypeMethodInfo MethodInfo { get { return _activation.MethodInfo; } }

        public CallRValue(CallActivation activation, Context context)
            : base(context)
        {
            if (activation == null)
                throw new ArgumentNullException("activation");

            _activation = activation;
        }


        public override void AssignInto(LValueProvider lValue)
        {
            generateCall();
            E.AssignReturnValue(lValue.Storage, MethodInfo.ReturnType);
        }

        public override void Return()
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
                argVariables.Add(arg.GetStorage());
            }

            var args = Arguments.Values(argVariables.ToArray());

            if (_activation.MethodInfo.IsStatic)
            {
                E.StaticCall(MethodInfo.DeclaringType, MethodInfo.MethodID, args);
            }
            else
            {
                var objStorage = _activation.CalledObject.GetStorage();

                var builder = E.Call(MethodInfo.MethodID, objStorage, args);
                builder.SetTransformationProvider(new CallProvider(_activation.CallNode));
            }
        }

        public override InstanceInfo GetResultInfo()
        {
            return MethodInfo.ReturnType;
        }

        public override string GetStorage()
        {
            var tmp = new TemporaryVariableValue(Context);
            AssignInto(tmp);

            return tmp.Storage;
        }

        public override void Generate()
        {
            generateCall();
        }
    }

    class TemporaryRVariableValue : RValueProvider
    {
        private readonly string _storage;

        public TemporaryRVariableValue(Context context, string storage = null)
            : base(context)
        {
            if (storage == null)
            {
                _storage = E.GetTemporaryVariable();
            }
            else
            {
                _storage = storage;
            }

        }

        public override void AssignInto(LValueProvider lValue)
        {
            throw new NotImplementedException();
        }

        public override void Return()
        {
            E.Return(_storage);
        }

        public override InstanceInfo GetResultInfo()
        {
            return E.VariableInfo(_storage);
        }

        public override string GetStorage()
        {
            return _storage;
        }

        public override void Generate()
        {
            throw new NotImplementedException();
        }
    }
}
