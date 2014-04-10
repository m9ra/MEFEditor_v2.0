using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using Analyzing.Editing;
using TypeSystem;
using AssemblyProviders.CSharp.Compiling;
using AssemblyProviders.CSharp.Interfaces;

namespace AssemblyProviders.CSharp.Transformations
{
    class EditContext : ExecutionViewData
    {
        private readonly HashSet<INodeAST> _removedNodes = new HashSet<INodeAST>();

        private readonly Source _source;

        private readonly ExecutionView _view;

        internal readonly StripManager Strips;

        public bool IsCommited { get; private set; }

        public string Code { get; private set; }

        internal EditContext(ExecutionView view, Source source, string code)
        {
            _view = view;
            Code = code;
            Strips = new StripManager(Code);
            _source = source;
        }

        internal EditContext(EditContext toClone)
        {
            _removedNodes = new HashSet<INodeAST>(toClone._removedNodes);
            _source = toClone._source;
            Strips = new StripManager(toClone.Strips);
            IsCommited = toClone.IsCommited;
            Code = toClone.Code;
        }



        internal void NodeRemoved(INodeAST removedNode)
        {
            _removedNodes.Add(removedNode);
        }


        protected override void commit()
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

            _source.OnCommited(this);
        }

        protected override ExecutionViewData clone()
        {
            return new EditContext(this);
        }

        private void checkVariableRemoving(VariableInfo variable)
        {
            foreach (var usage in variable.VariableUsings)
            {
                if (!_removedNodes.Contains(usage))
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
            var top = getTopNode(varUse);
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
            var assigns = variable.VariableAssigns;

            //remove all variable ussings before assign that can be redeclared
            INodeAST redeclaredAssign = null;
            foreach (var varUsing in variable.VariableUsings)
            {
                if (_removedNodes.Contains(varUsing))
                {
                    continue;
                }

                if (assigns.Contains(varUsing))
                {
                    //we found using that can be used for redeclaration
                    redeclaredAssign = varUsing;
                    break;
                }
                else
                {
                    _source.RemoveNode(_view, varUsing, false);
                }
            }

            if (redeclaredAssign == null)
                //there is nothing to be redeclared
                return;

            var redeclarationPoint = getRedeclarationNode(redeclaredAssign);
            var beforeStatementOffset = _source.BeforeStatementOffset(redeclarationPoint);
            var isUninitializedDeclaration = redeclaredAssign != redeclarationPoint;

            var redeclarationType = resolveRedeclarationType(variable, redeclaredAssign, !isUninitializedDeclaration);

            var toWrite = redeclarationType + " ";
            if (isUninitializedDeclaration)
            {
                //redeclare on previous line with full variable name
                toWrite += variable.Name + ";\n";
            }


            Strips.Write(beforeStatementOffset, toWrite);
        }

        private string resolveRedeclarationType(VariableInfo variable, INodeAST assignedVariable, bool canUseImpicit)
        {
            var variableType = resolveVariableType(variable);
            if (variableType == null)
            {
                throw new NotSupportedException("Cannot redeclare variable, because of missing type info");
            }


            if (!variable.IsImplicitlyTyped || !canUseImpicit)
            {
                //keep convetion on explicit variable typing
                //or we cannot use implicit typing e.g. because of uninitialized variable delcaration
                return variableType.TypeName;
            }

            var assignedType = resolveAssignType(assignedVariable);
            if (assignedType == null || assignedType.TypeName != variableType.TypeName)
            {
                //we don't know type of assignedType, or implicit type is different,
                //so whole type name is needed
                return variableType.TypeName;
            }

            //assigned type matches to variable type and implicit type convetion is used
            return "var";
        }

        private InstanceInfo resolveVariableType(VariableInfo variable)
        {
            if (!variable.IsImplicitlyTyped)
            {
                return variable.Type;
            }

            foreach (var assignedVar in variable.VariableUsings)
            {
                var type = resolveAssignType(assignedVar);
                if (type != null)
                    //first typed assign determine type
                    return type;
            }
            return null;
        }

        private InstanceInfo resolveAssignType(INodeAST assignedVariable)
        {
            var assignedNode = assignedVariable.Parent.Arguments[1];
            return resolveNodeType(assignedNode);
        }

        private InstanceInfo resolveNodeType(INodeAST node)
        {
            return _source.CompilationInfo.GetNodeType(node);
        }


    }
}
