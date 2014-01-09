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
    public class VariableInfo
    {
        public InstanceInfo Info { get; private set; }

        public readonly string Name;

        public readonly bool IsImplicitlyTyped;

        public bool IsArgument { get { return Declaration == null; } }

        internal readonly INodeAST Declaration;

        private readonly List<INodeAST> _variableUsings = new List<INodeAST>();

        internal VariableInfo(INodeAST declaration)
        {
            Declaration = declaration;
            Name = declaration.Arguments[1].Value;

            //TODO chained type names, namespace resolvings,..
            var typeName = declaration.Arguments[0].Value;
            if (typeName == "var")
            {
                IsImplicitlyTyped = true;
            }
            else
            {
                //not implicitly typed variable, we can determine type
                IsImplicitlyTyped = false;
                Info = TypeDescriptor.Create(typeName);
            }
        }

        internal VariableInfo(string variableName)
        {
            Name = variableName;
        }

        internal IEnumerable<INodeAST> VariableUsings
        {
            get
            {
                return _variableUsings;
            }
        }

        internal IEnumerable<INodeAST> VariableAssigns
        {
            get
            {
                var result = new List<INodeAST>();

                foreach (var varUsing in _variableUsings)
                {
                    var parent = varUsing.Parent;
                    if (parent == null)
                        continue;

                    if (parent.IsAssign() && parent.Arguments[0] == varUsing)
                    {
                        result.Add(varUsing);
                    }
                }

                return result;
            }
        }

        internal void AddVariableUsing(INodeAST variableUse)
        {
            _variableUsings.Add(variableUse);
        }

        internal void HintAssignedType(InstanceInfo info)
        {
            if (Info != null)
                //we already have type information
                return;

            Info = info;
        }
    }
}
