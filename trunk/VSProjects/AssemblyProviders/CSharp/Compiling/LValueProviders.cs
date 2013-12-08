using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeSystem;

using AssemblyProviders.CSharp.Interfaces;

namespace AssemblyProviders.CSharp.Compiling
{

    abstract class LValueProvider : ValueProvider
    {
        protected LValueProvider(Context context) : base(context) { }

        public abstract string Storage { get; }
    }


    class VariableValue : LValueProvider
    {
        private readonly VariableInfo _variable;
        private readonly INodeAST _variableNode;

        public VariableValue(VariableInfo variable, INodeAST variableNode, Context context)
            : base(context)
        {
            variable.AddVariableUsing(variableNode);
            _variable = variable;
            _variableNode = variableNode;
        }

        public override string Storage
        {
            get
            {
                return _variable.Name;
            }
        }
    }

    class TemporaryVariableValue : LValueProvider
    {
        private readonly string _storage;

        public TemporaryVariableValue(Context context, string storage = null)
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
        public override string Storage
        {
            get
            {
                return _storage;
            }
        }
    }
}
