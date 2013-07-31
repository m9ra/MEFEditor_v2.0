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

        protected IEmitter<MethodID, InstanceInfo> E { get { return Context.Emitter; } }

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
            //TODO get empty variable
            var temporaryName = ".temporary";
            E.AssignLiteral(temporaryName, _literal);
            E.Return(temporaryName);
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
    }

    class CallRValue : RValueProvider
    {
        private string _callName;
        public CallRValue(string callName, Context context)
            : base(context)
        {
            _callName =callName;
        }

        public override void AssignInto(LValueProvider lValue)
        {
            lValue.PreAction();
            generateCall();
            E.AssignReturnValue(lValue.Storage);    
            lValue.PostAction();
        }

        public override void Return()
        {
            //TODO get empty variable
            var temporaryName = ".temporary";            
            E.AssignReturnValue(temporaryName);
            E.Return(temporaryName);
        }

        private void generateCall()
        {
            //Resolve called object
            E.Call(new MethodID(_callName), "this");                    
        }
    }
}
