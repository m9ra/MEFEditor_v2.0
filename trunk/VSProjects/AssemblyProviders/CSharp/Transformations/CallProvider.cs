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
                var sideEffect = keepSideEffect && hasSideEffect(argNode);
                source.RemoveNode(view, argNode, sideEffect);
            }, argNode.Source);
        }

        public override Transformation RewriteArgument(int argumentIndex, ValueProvider valuePovider)
        {
            if (_call.Arguments.Length <= argumentIndex - 1)
                return null;

            var argNode = _call.Arguments[argumentIndex - 1];

            return new SourceTransformation((view, source) =>
            {
                var keepSideEffect = hasSideEffect(argNode);
                source.Rewrite(view, argNode, valuePovider(view), keepSideEffect);
            }, _call.Source);
        }

        public override Transformation AppendArgument(int argumentIndex, ValueProvider valueProvider)
        {
            var index = argumentIndex - 1;
            if (_call.Arguments.Length != index)
                //cannot append argument
                return null;

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
                source.RemoveNode(view, _call, false);
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
            if (node.Value == "typeof")
                return false;

            if (node.IsAssign())
                return true;

            //TODO: calls with namespaces
            return new NodeTypes[] { NodeTypes.call, NodeTypes.prefixOperator }.Contains(node.NodeType);
        }

        #endregion
    }
}
