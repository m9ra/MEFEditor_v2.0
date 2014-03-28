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
        protected LValueProvider(CompilationContext context) : base(context) { }

        public abstract string Storage { get; }

        public abstract TypeDescriptor Type { get; }
    }


    class VariableLValue : LValueProvider
    {
        private readonly VariableInfo _variable;
        private readonly INodeAST _variableNode;

        public VariableLValue(VariableInfo variable, INodeAST variableNode, CompilationContext context)
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

        public override TypeDescriptor Type
        {
            get { return _variable.Type; }
        }
    }

    class TemporaryVariableValue : LValueProvider
    {
        private readonly string _storage;

        private readonly TypeDescriptor _type;

        public TemporaryVariableValue(TypeDescriptor type, CompilationContext context, string storage = null)
            : base(context)
        {
            _type = type;

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

        public override TypeDescriptor Type
        {
            get { return _type; }
        }
    }
}
