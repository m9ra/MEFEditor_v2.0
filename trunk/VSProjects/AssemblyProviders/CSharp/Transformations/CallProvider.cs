using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            return new SourceTransformation(()=>
                source.Remove(argNode.StartingToken.Position, argNode.EndingToken.Next.Position)
            );

        }
    }
}
