using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AssemblyProviders.CSharp.Compiling;
using AssemblyProviders.CSharp.Interfaces;

namespace AssemblyProviders.CSharp.Transformations
{
    class EditContext
    {
        private HashSet<INodeAST> _removedVariableUsings = new HashSet<INodeAST>();
        private Dictionary<INodeAST, CallProvider> _callProviders = new Dictionary<INodeAST, CallProvider>();

        private readonly Source _source;

        internal readonly StripManager Strips;

        public bool IsCommited { get; private set; }
        public string Code { get; private set; }

        internal EditContext(Source source, string code)
        {
            Code = code;
            _source = source;
            Strips = new StripManager(code);
        }

        internal void RegisterCallProvider(INodeAST callNode, CallProvider callProvider)
        {
            _callProviders.Add(callNode, callProvider);
        }

        internal CallProvider GetProvider(INodeAST call)
        {
            return _callProviders[call];
        }



        internal void VariableNodeRemoved(INodeAST variableUse)
        {
            switch (variableUse.NodeType)
            {
                case NodeTypes.hierarchy:
                case NodeTypes.declaration:
                    break;

                default:
                    throw new NotSupportedException("Expecting variable usage node");
            }

            _removedVariableUsings.Add(variableUse);
        }

        internal void Commit()
        {
            if (IsCommited)
                //commit has already been processed
                return;

            foreach (var variable in _source.CompilationInfo.Variables)
            {
                checkVariableRemoving(variable);
            }
            Code = Strips.Data;
            IsCommited = true;
        }

        private void checkVariableRemoving(VariableInfo variable)
        {
            foreach (var usage in variable.VariableAssigns)
            {
                if (!_removedVariableUsings.Contains(usage))
                    continue;


                if (variable.Declaration == usage)
                {
                    //it needs to be redeclared
                    redeclare(variable);
                    break;
                }
            }
        }

        private INodeAST getTopNode(INodeAST node)
        {
            var currNode = node;

            while (currNode.Parent != null)
            {
                currNode = currNode.Parent;
            }
            return currNode;
        }

        private INodeAST getRedeclarationNode(INodeAST varUse)
        {
            var top=getTopNode(varUse);
            if (top.Arguments[0] != varUse)
            {
                //redeclaration on previous line
                return top;
            }

            //we can prepend variable use with type
            return varUse;
        }

        private void redeclare(VariableInfo variable)
        {
            var leavedUsages = variable.VariableAssigns.Except(_removedVariableUsings);
            if (!leavedUsages.Any())
            {
                //variable has been completly removed - it doesn't need to be redeclared
                return;
            }

            var variableUse = leavedUsages.First();
            var redeclarationPoint = getRedeclarationNode(variableUse);
            var beforeStatementOffset = _source.BeforeStatementOffset(redeclarationPoint);


            var toWrite = variable.Info.TypeName + " ";
            if (variableUse != redeclarationPoint)
            {
                toWrite += variable.Name+";\n";
            }


            Strips.Write(beforeStatementOffset,toWrite);
        }


    }
}
