using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Editing;
using AssemblyProviders.CSharp.Interfaces;

namespace AssemblyProviders.CSharp.Transformations
{
    class AssignRemove:RemoveTransformProvider
    {
        private INodeAST _assignedValue;

        public AssignRemove(INodeAST assignedValue)
        {
            _assignedValue = assignedValue;
        }
        public override Transformation Remove()
        {
            var assignOperator=_assignedValue.Parent;
            if (assignOperator.NodeType == NodeTypes.declaration)
            {
                throw new NotImplementedException();
            }

            if (assignOperator.Parent != null)
            {
                throw new NotImplementedException();
            }

            var variableNode = assignOperator.Arguments[0];

            //remove whole statement;
            return new SourceTransformation((view,source) =>
            {
                source.EditContext(view).VariableNodeRemoved(variableNode);
                source.Remove(view,assignOperator, false);
            },_assignedValue.Source);
        }
    }
}
