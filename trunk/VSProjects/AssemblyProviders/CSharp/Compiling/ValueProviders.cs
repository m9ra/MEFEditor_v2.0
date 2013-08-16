using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using TypeSystem;

using AssemblyProviders.CSharp.Interfaces;

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

        internal void PreAction()
        {
        }

        internal void PostAction()
        {
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

        
    }


    class VariableValue : LValueProvider
    {
        private string _variableName;

        public VariableValue(string variableName, Context context)
            : base(context)
        {
            this._variableName = variableName;
        }


        public override string Storage { get { return _variableName; } }
    }

    class LiteralValue : RValueProvider
    {
        private object _literal;
        public LiteralValue(object literal, Context context)
            : base(context)
        {
            _literal = literal;
        }

        public override void AssignInto(LValueProvider lValue)
        {
            lValue.PreAction();
            E.AssignLiteral(lValue.Storage, _literal);
            lValue.PostAction();
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
            lValue.PreAction();
            E.Assign(lValue.Storage, _variableName);
            lValue.PostAction();
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
    }

    class CallRValue : RValueProvider
    {
        private TypeMethodInfo _methodInfo;
        private RValueProvider[] _arguments;

        public CallRValue(TypeMethodInfo methodInfo,RValueProvider[] arguments, Context context)
            : base(context)
        {
            _methodInfo = methodInfo;
            _arguments = arguments;
        }

        public override void AssignInto(LValueProvider lValue)
        {
            lValue.PreAction();
            foreach (var arg in _arguments)
            {
                arg.PreAction();
            }

            generateCall();            
            E.AssignReturnValue(lValue.Storage);

            foreach (var arg in _arguments)
            {
                arg.PostAction();
            }
            lValue.PostAction();    
        }

        public override void Return()
        {
            //TODO get empty variable
            var temporaryName = E.GetTemporaryVariable();            
            E.AssignReturnValue(temporaryName);
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
                //TODO Resolve called object
                E.Call(new MethodID(_methodInfo.Path),"this", argVariables.ToArray());
            }
        }

        public override InstanceInfo GetResultInfo()
        {
            throw new NotImplementedException();
        }

        public override string GetStorage()
        {
            throw new NotImplementedException();
        }
    }
}
