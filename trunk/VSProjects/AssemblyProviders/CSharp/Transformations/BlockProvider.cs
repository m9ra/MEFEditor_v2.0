using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AssemblyProviders.CSharp.Interfaces;

using Analyzing.Editing;

namespace AssemblyProviders.CSharp.Transformations
{
    class BlockProvider:BlockTransformationProvider
    {
        readonly INodeAST _line;
        internal BlockProvider(INodeAST lineNode){
            _line = lineNode;
        }

        public override Transformation ShiftBefore(BlockTransformationProvider provider)
        {
            throw new NotImplementedException();
        }

        public override Transformation ShiftBehind(BlockTransformationProvider provider)
        {
            var other = provider as BlockProvider;
            var lineSource = _line.Source;

            if (lineSource != other._line.Source)
            {
                throw new NotSupportedException("Cannot shift between sources");
            }

            return new SourceTransformation((t,source) =>
            {
                source.ShiftBehind(_line, other._line);
            },lineSource);
        }

        public override Transformation PrependCall(CallEditInfo call)
        {
            throw new NotImplementedException();
        }

        public override Transformation AppendCall(CallEditInfo call)
        {
            throw new NotImplementedException();
        }
    }
}
