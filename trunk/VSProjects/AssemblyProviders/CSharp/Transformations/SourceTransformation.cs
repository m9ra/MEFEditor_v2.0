using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Editing;
namespace AssemblyProviders.CSharp.Transformations
{
    delegate void TransformAction(TransformationServices services);

    class SourceTransformation:Transformation
    {
        readonly TransformAction _apply;
        internal SourceTransformation(TransformAction apply)
        {
            if (apply == null)
                throw new ArgumentNullException();
            _apply = apply;
        }
        protected override void apply(TransformationServices services)
        {
            _apply(services);
        }
    }
}
