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
    /// <summary>
    /// Determine that LValue provide storage which can be read
    /// <remarks>This is needed for assigning new objects for constructor call</remarks>
    /// </summary>
    interface IStorageReadProvider
    {
        string Storage { get; }
    }

    abstract class LValueProvider : ValueProvider
    {
        protected LValueProvider(CompilationContext context) : base(context) { }

        public abstract TypeDescriptor Type { get; }

        public abstract void AssignNewObject(TypeDescriptor newObjectType, INodeAST newObjectNode);

        public abstract void AssignLiteral(object literal, INodeAST literalNode);

        public abstract void AssignReturnValue(TypeDescriptor returnedType, INodeAST callNode);

        public abstract void Assign(string variable, INodeAST variableNode);
    }

    class SetterLValue : LValueProvider
    {
        /// <summary>
        /// Position specification of set value. Used for indexers.
        /// </summary>
        private readonly RValueProvider[] _positionArguments;

        /// <summary>
        /// This object if call is not static
        /// </summary>
        private readonly RValueProvider _thisObjet;

        /// <summary>
        /// Method of setter
        /// </summary>
        private readonly TypeMethodInfo _setter;

        public SetterLValue(TypeMethodInfo setter, RValueProvider thisObject, IEnumerable<RValueProvider> positionalArguments, CompilationContext context)
            : base(context)
        {
            _setter = setter;
            _thisObjet = thisObject;
            //defensive copy
            _positionArguments = positionalArguments.ToArray();

            if (!_setter.IsStatic && thisObject == null)
                throw new ArgumentNullException("thisObject");
        }

        public override TypeDescriptor Type
        {
            get { return TypeDescriptor.Void; }
        }

        public override void AssignNewObject(TypeDescriptor newObjectType, INodeAST newObjectNode)
        {
            throw new NotImplementedException();
        }

        public override void AssignLiteral(object literal, INodeAST literalNode)
        {
            var storage = E.GetTemporaryVariable("const");
            E.AssignLiteral(storage, literal);
            var builder = generateAssign(storage);
            //TODO set call transformation provider
        }

        public override void AssignReturnValue(TypeDescriptor returnedValueType, INodeAST callNode)
        {
            var storage = E.GetTemporaryVariable("ret");
            var builder = E.AssignReturnValue(storage, returnedValueType);
            builder.RemoveProvider = new AssignRemove(callNode);
            generateAssign(storage);
            //TODO set call transformation provider
        }

        public override void Assign(string variable, INodeAST variableNode)
        {
            generateAssign(variable);
            //TODO set call transformation provider
        }

        private CallBuilder generateAssign(string source)
        {
            var arguments = new List<string>();
            foreach (var position in _positionArguments)
            {
                var storage = position.GenerateStorage();
                arguments.Add(storage);
            }

            arguments.Add(source);

            var argumentStorages = Arguments.Values(arguments);
            if (_setter.IsStatic)
            {
                return E.StaticCall(_setter.DeclaringType, _setter.MethodID, argumentStorages);
            }
            else
            {
                var thisObjStorage = _thisObjet.GenerateStorage();
                return E.Call(_setter.MethodID, thisObjStorage, argumentStorages);
            }
        }
    }

    class VariableLValue : LValueProvider, IStorageReadProvider
    {
        private readonly VariableInfo _variable;
        private readonly INodeAST _variableNode;

        public virtual string Storage { get { return _variable.Name; } }

        public override TypeDescriptor Type { get { return _variable.Type; } }

        public VariableLValue(VariableInfo variable, INodeAST variableNode, CompilationContext context)
            : base(context)
        {
            variable.AddVariableUsing(variableNode);
            _variable = variable;
            _variableNode = variableNode;
        }

        protected VariableLValue(CompilationContext context)
            : base(context)
        {
            //only for inheritance purposes
            //note that Storage and Type has to be overriden
        }

        public override void AssignNewObject(TypeDescriptor newObjectType, INodeAST newObjectNode)
        {
            var builder = E.AssignNewObject(Storage, newObjectType);
            builder.RemoveProvider = new AssignRemove(newObjectNode);
        }

        public override void AssignLiteral(object literal, INodeAST literalNode)
        {
            var builder = E.AssignLiteral(Storage, literal);
            builder.RemoveProvider = new AssignRemove(literalNode);
        }

        public override void AssignReturnValue(TypeDescriptor returnedType, INodeAST callNode)
        {
            var builder = E.AssignReturnValue(Storage, returnedType);
            builder.RemoveProvider = new AssignRemove(callNode);
        }

        public override void Assign(string variable, INodeAST variableNode)
        {
            var builder = E.Assign(Storage, variable);
            builder.RemoveProvider = new AssignRemove(variableNode);
        }
    }

    class TemporaryVariableValue : VariableLValue, IStorageReadProvider
    {
        internal readonly string _storage;

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
            get { return _storage; }
        }

        public override TypeDescriptor Type
        {
            get { return _type; }
        }
    }
}
