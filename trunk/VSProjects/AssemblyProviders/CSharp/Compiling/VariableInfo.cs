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
    /// <summary>
    /// Representation of information that is known about variable
    /// </summary>
    public class VariableInfo
    {
        /// <summary>
        /// Nodes where variable has been used
        /// </summary>
        private readonly List<INodeAST> _variableUsings = new List<INodeAST>();

        /// <summary>
        /// Compilation info belonging to context where variable has been declared
        /// </summary>
        private readonly CompilationInfo _info;

        /// <summary>
        /// Type of variable
        /// </summary>
        private TypeDescriptor _type;

        /// <summary>
        /// Currently known type of variable. 
        /// <remarks>It may change during time and can be null for implicitly typed variables</remarks>
        /// </summary>
        public TypeDescriptor Type
        {
            get
            {
                if (_type == null)
                {
                    //find type between usings
                    foreach (var use in _variableUsings)
                    {
                        _type = _info.ResolveAssignType(use);
                        if (_type != null)
                            break;
                    }
                }

                return _type;
            }
            private set { _type = value; }
        }

        /// <summary>
        /// Name of represented variables
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Determine that variable is implicitly typed (var keyword in C#)
        /// </summary>
        public readonly bool IsImplicitlyTyped;

        /// <summary>
        /// Declaration node of variable
        /// </summary>
        internal readonly INodeAST Declaration;

        /// <summary>
        /// Initialize <see cref="VariableInfo"/> object from variable declaration
        /// </summary>
        internal VariableInfo(INodeAST declaration, TypeDescriptor declaredType, CompilationInfo info)
        {
            _info = info;
            Declaration = declaration;
            Name = declaration.Arguments[1].Value;

            //TODO chained type names, namespace resolvings,..
            IsImplicitlyTyped = declaration.Arguments[0].Value == LanguageDefinitions.CSharpSyntax.ImplicitVariableType;
            Type = declaredType;

            if (!IsImplicitlyTyped && Type == null)
                throw new NotSupportedException("Cannot create non implicit declaration without type");
        }

        /// <summary>
        /// Initialize <see cref="VariableInfo"/> object from name of method argument.
        /// <remarks>Declaration node is not available for method arguments</remarks>
        /// </summary>
        /// <param name="argumentName">Name of argument defining current variable</param>
        internal VariableInfo(string argumentName, CompilationInfo info)
        {
            _info = info;
            Name = argumentName;
        }

        /// <summary>
        /// Nodes where variable has been used
        /// </summary>
        internal IEnumerable<INodeAST> VariableUsings
        {
            get
            {
                return _variableUsings;
            }
        }

        /// <summary>
        /// Nodes where variable were assigned
        /// </summary>
        internal IEnumerable<INodeAST> VariableAssigns
        {
            get
            {
                var result = new List<INodeAST>();

                foreach (var varUsing in _variableUsings)
                {
                    var parent = varUsing.Parent;
                    if (parent == null)
                        //using is not an assign
                        continue;

                    //determine that variable is assign target not a source
                    if (parent.IsAssign() && parent.Arguments[0] == varUsing)
                    {
                        result.Add(varUsing);
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Hint of type of value assigned to represented variable
        /// that can be used for defining type of variable. 
        /// In case of <see cref="IsImplicitlyTyped"/> or if it is argument.
        /// </summary>
        /// <param name="type">Assigned type</param>
        internal void HintAssignedType(TypeDescriptor type)
        {
            if (Type != null)
                //we already have type information
                return;

            //we does not have type yet
            Type = type;
        }

        /// <summary>
        /// Add variable using into list
        /// </summary>
        /// <param name="variableUse">Node where variable is used</param>
        internal void AddVariableUsing(INodeAST variableUse)
        {
            _variableUsings.Add(variableUse);
        }


    }
}
