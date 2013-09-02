using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TypeSystem;
using AssemblyProviders.CSharp.Interfaces;

namespace AssemblyProviders.CSharp.Compiling
{
    public class VariableInfo
    {
        public readonly InstanceInfo Info;

        public readonly string Name;

        public bool IsArgument { get { return Declaration == null; } }

        internal readonly INodeAST Declaration;

        internal readonly List<INodeAST> VariableAssigns = new List<INodeAST>();

        internal VariableInfo(INodeAST declaration,InstanceInfo info)
        {
            Info = info;
            Declaration = declaration;
            Name = declaration.Arguments[1].Value;
            VariableAssigns.Add(declaration);
        }

        internal VariableInfo(string variableName, InstanceInfo info)
        {
            Name = variableName;
            Info = info;
        }
    }
}
