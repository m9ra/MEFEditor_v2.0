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

        public override Transformation RemoveArgument(int argumentIndex)
        {
            var argNode = _call.Arguments[argumentIndex - 1];
            var source=_call.StartingToken.Position.Source;

            return new SourceTransformation((s)=>{                
                var keepSideEffect=! hasSideEffect(argNode);
                source.Remove(argNode, keepSideEffect);
            });
        }

        public override Transformation RewriteArgument(int argumentIndex,ValueProvider valuePovider)
        {
            var argNode = _call.Arguments[argumentIndex - 1];
            var source = _call.StartingToken.Position.Source;

            return new SourceTransformation((services) =>
            {
                var keepSideEffect = !hasSideEffect(argNode);
                source.Rewrite(argNode,valuePovider(services), keepSideEffect);
            });
        }

        public override Transformation AppendArgument(ValueProvider valueProvider)
        {            
            var source = _call.StartingToken.Position.Source;

            return new SourceTransformation((services) =>
            {
                var value = valueProvider(services);
                if (services.IsAborted) 
                    return;

                source.AppendArgument(_call, value);                
            });
        }

        private bool hasSideEffect(INodeAST node)
        {  
            //TODO: binary side effect has only assign
            return new NodeTypes[] { NodeTypes.hierarchy, NodeTypes.prefixOperator }.Contains(node.NodeType);
        }


    }
}
