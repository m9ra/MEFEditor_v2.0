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

        private readonly List<INodeAST> _variableAssigns = new List<INodeAST>();

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
                Info = new InstanceInfo(typeName);
            }
        }

        internal VariableInfo(string variableName)
        {
            Name = variableName;            
        }

        internal IEnumerable<INodeAST> VariableAssigns
        {
            get
            {
                return _variableAssigns;
            }
        }

        internal void AddVariableUse(INodeAST variableAssign)
        {
            _variableAssigns.Add(variableAssign);
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
