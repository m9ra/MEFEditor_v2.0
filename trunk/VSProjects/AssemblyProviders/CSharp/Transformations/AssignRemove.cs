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
        private INodeAST _literalNode;

        public AssignRemove(INodeAST assignedValue)
        {
            _literalNode = assignedValue;
        }
        public override Transformation Remove()
        {
            var assignOperator=_literalNode.Parent;
            if (assignOperator.NodeType == NodeTypes.declaration)
            {
                throw new NotImplementedException();
            }

            if (assignOperator.Parent != null)
            {
                throw new NotImplementedException();
            }

            //remove whole satement;
            return new SourceTransformation((c,source) =>
            {
                source.Remove(assignOperator, false);
            },_literalNode.Source);
        }
    }
}
