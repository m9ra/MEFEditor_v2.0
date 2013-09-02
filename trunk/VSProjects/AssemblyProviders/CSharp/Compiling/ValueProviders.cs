using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using TypeSystem;

using AssemblyProviders.CSharp.Interfaces;
using AssemblyProviders.CSharp.Transformations;

namespace AssemblyProviders.CSharp.Compiling
{

    abstract class ValueProvider
    {
        protected readonly Context Context;

        protected EmitterBase<MethodID, InstanceInfo> E { get { return Context.Emitter; } }

        protected ValueProvider(Context context)
        {
            Context = context;
        }
    }

    abstract class LValueProvider : ValueProvider
    {
        protected LValueProvider(Context context) : base(context) { }

        public abstract string Storage { get; }
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


    class VariableValue : LValueProvider
    {
        private string _variableName;

        public VariableValue(VariableInfo variable, INodeAST variableNode, Context context)
            : base(context)
        {
            variable.VariableAssigns.Add(variableNode);
            this._variableName = variable.Name;
        }


        public override string Storage { get { return _variableName; } }
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
            _storage=lValue.Storage;
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
        private readonly INodeAST _literalNode;
        public LiteralValue(object literal,INodeAST literalNode, Context context)
            : base(context)
        {
            _literal = literal;
            _literalNode = literalNode;
        }

        public override void AssignInto(LValueProvider lValue)
        {            
            var builder=E.AssignLiteral(lValue.Storage, _literal);
            builder.RemoveProvider = new AssignRemove(_literalNode);
        }

        public override void Return()
        {
            E.Return(GetStorage());
        }

        public override InstanceInfo GetResultInfo()
        {
            throw new NotImplementedException();
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

    class VariableRValue: RValueProvider
    {
        private string _variableName;
        public VariableRValue(string variableName, Context context)
            : base(context)
        {
            _variableName = variableName;
        }

        public override void AssignInto(LValueProvider lValue)
        {            
            E.Assign(lValue.Storage, _variableName);         
        }
        
        public override void Return()
        {
            E.Return(_variableName);
        }

        public override InstanceInfo GetResultInfo()
        {
            return E.VariableInfo(_variableName);
        }

        public override string GetStorage()
        {
            return _variableName;
        }

        public override void Generate()
        {
            //Nothing to generate
        }
    }

    class CallRValue : RValueProvider
    {
        private TypeMethodInfo _methodInfo;
        private RValueProvider[] _arguments;
        private INodeAST _callNode;
        private RValueProvider _calledObject;

        public CallRValue(INodeAST callNode,TypeMethodInfo methodInfo,RValueProvider calledObject,RValueProvider[] arguments, Context context)
            : base(context)
        {
            _callNode = callNode;
            _methodInfo = methodInfo;
            _arguments = arguments;
            _calledObject = calledObject;

            if (_calledObject == null && !methodInfo.IsStatic)
            {
                throw new NotSupportedException();
            }
        }


        public override void AssignInto(LValueProvider lValue)
        {
            generateCall();            
            E.AssignReturnValue(lValue.Storage,_methodInfo.ReturnType);
        }

        public override void Return()
        {            
            var temporaryName = E.GetTemporaryVariable();            
            E.AssignReturnValue(temporaryName,_methodInfo.ReturnType);
            E.Return(temporaryName);
        }

        private void generateCall()
        {
            var argVariables = new List<string>();
            foreach (var arg in _arguments)
            {
                argVariables.Add(arg.GetStorage());
            }
            
            if (_methodInfo.IsStatic)
            {            
                E.StaticCall(_methodInfo.TypeName,new MethodID(_methodInfo.Path), argVariables.ToArray());
            }
            else
            {
                var objStorage = _calledObject.GetStorage();                
                
                var builder=E.Call(new MethodID(_methodInfo.Path),objStorage, argVariables.ToArray());
                builder.SetTransformationProvider(new CallProvider(_callNode));
            }
        }

        public override InstanceInfo GetResultInfo()
        {
            return _methodInfo.ReturnType;
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

    class TemporaryVariableValue:LValueProvider
    {
        private readonly string _storage;

        public TemporaryVariableValue(Context context)
            : base(context)
        {
            _storage = E.GetTemporaryVariable();
        }
        public override string Storage
        {
            get { return _storage; }
        }
    }
}
