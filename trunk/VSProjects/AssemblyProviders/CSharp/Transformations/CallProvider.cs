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
        private readonly INodeAST _call;
        private HashSet<int> _optionals;

        internal CallProvider(INodeAST callNode)
        {
            _call = callNode;
            _call.Source.EditContext.RegisterCallProvider(callNode, this);
        }

        public override RemoveTransformProvider RemoveArgument(int argumentIndex, bool keepSideEffect)
        {
            var argNode = _call.Arguments[argumentIndex - 1];

            return new SourceRemoveProvider((s, source) =>
            {
                var sideEffect = keepSideEffect && !hasSideEffect(argNode);
                source.Remove(argNode, sideEffect);
            }, argNode.Source);
        }

        public override Transformation RewriteArgument(int argumentIndex, ValueProvider valuePovider)
        {
            var argNode = _call.Arguments[argumentIndex - 1];

            return new SourceTransformation((services, source) =>
            {
                var keepSideEffect = !hasSideEffect(argNode);
                source.Rewrite(argNode, valuePovider(services), keepSideEffect);
            }, _call.Source);
        }

        public override Transformation AppendArgument(ValueProvider valueProvider)
        {
            return new SourceTransformation((services, source) =>
            {
                var value = valueProvider(services);
                if (services.IsAborted)
                    return;

                source.AppendArgument(_call, value);
            }, _call.Source);
        }




        public override RemoveTransformProvider Remove()
        {
            return new SourceRemoveProvider((s, source) =>
            {
                keepParent(_call);
                source.Remove(_call, false);
            }, _call.Source);
        }

        public override bool IsOptionalArgument(int argumentIndex)
        {
            if (_optionals == null)
                //no argument is optional
                return false;

            return _optionals.Contains(argumentIndex);
        }

        public override void SetOptionalArgument(int index)
        {
            if (_optionals == null)
                _optionals = new HashSet<int>();

            _optionals.Add(index);
        }

        #region Private helpers

        private bool hasSideEffect(INodeAST node)
        {
            //TODO: binary side effect has only assign
            return new NodeTypes[] { NodeTypes.hierarchy, NodeTypes.prefixOperator }.Contains(node.NodeType);
        }

        private void keepParent(INodeAST node)
        {
            var parent = node.Parent;

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
                    }
                    parent.Source.Remove(parent, false);
                    keepParent(parent);
                    break;
                case NodeTypes.call:
                    if (!isOptionalArgument(parent, node))
                    {
                        parent.Source.Remove(parent, false);
                        keepParent(parent);
                    }
                    break;
                case NodeTypes.hierarchy:
                    parent.Source.Remove(parent, false);
                    keepParent(parent);
                    break;

                default:

                    throw new NotImplementedException();
            }

        }

        private int getArgumentIndex(INodeAST call, INodeAST argument)
        {
            for (int i = 0; i < call.Arguments.Length; ++i)
            {
                if (call.Arguments[i] == argument)
                    return i;
            }
            throw new NotSupportedException("Given argument is not node of given call");
        }

        private bool isOptionalArgument(INodeAST call, INodeAST argument)
        {
            var provider = call.Source.EditContext.GetProvider(call);
            var index = getArgumentIndex(call, argument);
            return provider.IsOptionalArgument(index + 1);
        }
        #endregion
    }
}
