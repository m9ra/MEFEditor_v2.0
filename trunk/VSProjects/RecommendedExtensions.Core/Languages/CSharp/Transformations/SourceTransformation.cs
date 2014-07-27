using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing.Editing;
namespace RecommendedExtensions.Core.Languages.CSharp.Transformations
{
    delegate void TransformAction(ExecutionView view, Source source);

    class SourceTransformation : Transformation
    {
        readonly Source _source;
        readonly TransformAction _apply;
        internal SourceTransformation(TransformAction apply, Source source)
        {
            if (apply == null)
                throw new ArgumentNullException();

            _apply = apply;
            _source = source;
        }
        protected override void apply()
        {
            _apply(View, _source);
        }
    }
}
