using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AssemblyProviders.CSharp.Interfaces;

using Analyzing.Editing;

namespace AssemblyProviders.CSharp.Transformations
{
    class BlockProvider : BlockTransformationProvider
    {
        readonly INodeAST _line;
        readonly Source _source;
        internal BlockProvider(INodeAST lineNode, Source source = null)
        {
            _line = lineNode;
            _source = _line == null ? source : _line.Source;

            if (_source == null)
                throw new ArgumentNullException("source");
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

            return new SourceTransformation((view, source) =>
            {
                source.ShiftBehind(view, _line, other._line);
            }, lineSource);
        }

        public override Transformation PrependCall(CallEditInfo call)
        {
            return new SourceTransformation((view, source) =>
            {
                source.PrependCall(view, _line, call);
            }, _line.Source);
        }

        public override Transformation AppendCall(CallEditInfo call)
        {
            return new SourceTransformation((view, source) =>
            {
                source.AppendCall(view, _line, call);
            }, _line.Source);
        }
    }
}
