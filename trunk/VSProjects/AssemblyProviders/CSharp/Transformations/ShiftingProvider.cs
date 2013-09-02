using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AssemblyProviders.CSharp.Interfaces;

using Analyzing.Editing;

namespace AssemblyProviders.CSharp.Transformations
{
    class ShiftingProvider:ShiftingTransformationProvider
    {
        readonly INodeAST _line;
        internal ShiftingProvider(INodeAST lineNode){
            _line = lineNode;
        }

        public override Transformation ShiftBefore(ShiftingTransformationProvider provider)
        {
            throw new NotImplementedException();
        }

        public override Transformation ShiftBehind(ShiftingTransformationProvider provider)
        {
            var other = provider as ShiftingProvider;
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
    }
}
