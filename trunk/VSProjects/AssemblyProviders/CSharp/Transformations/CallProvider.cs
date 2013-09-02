using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using Analyzing.Editing;

using AssemblyProviders.CSharp.Interfaces;

namespace AssemblyProviders.CSharp.Transformations
{
    class CallProvider : CallTransformProvider
    {
        readonly INodeAST _call;
        internal CallProvider(INodeAST callNode)
        {
            _call = callNode;
        }

        public override RemoveTransformProvider RemoveArgument(int argumentIndex)
        {
            var argNode = _call.Arguments[argumentIndex - 1];

            return new SourceRemoveProvider((s,source) =>
            {
                var keepSideEffect = !hasSideEffect(argNode);
                source.Remove(argNode, keepSideEffect);
            },argNode.Source);
        }

        public override Transformation RewriteArgument(int argumentIndex, ValueProvider valuePovider)
        {
            var argNode = _call.Arguments[argumentIndex - 1];

            return new SourceTransformation((services,source) =>
            {
                var keepSideEffect = !hasSideEffect(argNode);
                source.Rewrite(argNode, valuePovider(services), keepSideEffect);
            },_call.Source);
        }

        public override Transformation AppendArgument(ValueProvider valueProvider)
        {
            return new SourceTransformation((services,source) =>
            {
                var value = valueProvider(services);
                if (services.IsAborted)
                    return;

                source.AppendArgument(_call, value);
            }, _call.Source);
        }

        private bool hasSideEffect(INodeAST node)
        {
            //TODO: binary side effect has only assign
            return new NodeTypes[] { NodeTypes.hierarchy, NodeTypes.prefixOperator }.Contains(node.NodeType);
        }

        private void keepParent(INodeAST parent)
        {
            if (parent == null)
            {
                //there is no action for keeping parent
                return;
            }

            switch (parent.NodeType)
            {
                case NodeTypes.binaryOperator:
                    if (parent.IsAssign())
                    {
                        //report assigned variable change
                        parent.Source.EditContext.VariableNodeRemoved(parent.Arguments[0]);
                        parent.Source.Remove(parent, false);
                    }
                    keepParent(parent.Parent);
                    break;
                default:
                    throw new NotImplementedException();
            }

        }

        public override RemoveTransformProvider Remove()
        {

            return new SourceRemoveProvider((s,source) =>
            {
                keepParent(_call.Parent);
                source.Remove(_call, false);
            },_call.Source);
        }

        public override bool IsOptionalArgument(int argumentIndex)
        {
            //TODO 
            return false;
        }
    }
}
