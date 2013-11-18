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
            _call.Source.CompilationInfo.RegisterCallProvider(callNode, this);
        }

        public override RemoveTransformProvider RemoveArgument(int argumentIndex, bool keepSideEffect)
        {
            var argNode = _call.Arguments[argumentIndex - 1];

            return new SourceRemoveProvider((view, source) =>
            {
                var sideEffect = keepSideEffect && !hasSideEffect(argNode);
                source.Remove(view, argNode, sideEffect);
            }, argNode.Source);
        }

        public override Transformation RewriteArgument(int argumentIndex, ValueProvider valuePovider)
        {
            var argNode = _call.Arguments[argumentIndex - 1];

            return new SourceTransformation((view, source) =>
            {
                var keepSideEffect = !hasSideEffect(argNode);
                source.Rewrite(view, argNode, valuePovider(view), keepSideEffect);
            }, _call.Source);
        }

        public override Transformation AppendArgument(ValueProvider valueProvider)
        {
            return new SourceTransformation((view, source) =>
            {
                var value = valueProvider(view);
                if (view.IsAborted)
                    return;

                source.AppendArgument(view, _call, value);
            }, _call.Source);
        }




        public override RemoveTransformProvider Remove()
        {
            return new SourceRemoveProvider((view, source) =>
            {
                keepParent(view, _call);
                source.Remove(view, _call, false);
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

        private void keepParent(ExecutionView view, INodeAST node)
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
                        parent.Source.EditContext(view).VariableNodeRemoved(parent.Arguments[0]);
                    }
                    parent.Source.Remove(view, parent, false);
                    keepParent(view, parent);
                    break;
                case NodeTypes.call:
                    if (!isOptionalArgument(parent, node))
                    {
                        parent.Source.Remove(view, parent, false);
                        keepParent(view, parent);
                    }
                    break;
                case NodeTypes.hierarchy:
                    parent.Source.Remove(view, parent, false);
                    keepParent(view, parent);
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
            var provider = call.Source.CompilationInfo.GetProvider(call);
            var index = getArgumentIndex(call, argument);
            return provider.IsOptionalArgument(index + 1);
        }
        #endregion
    }
}
