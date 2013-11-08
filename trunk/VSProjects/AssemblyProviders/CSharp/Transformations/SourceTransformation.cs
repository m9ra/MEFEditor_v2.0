using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Editing;
namespace AssemblyProviders.CSharp.Transformations
{
    delegate void TransformAction(TransformationServices services,Source source);

    class SourceTransformation:Transformation
    {
        readonly Source _source;
        readonly TransformAction _apply;
        internal SourceTransformation(TransformAction apply,Source source)
        {
            if (apply == null)
                throw new ArgumentNullException();
            _apply = apply;
            _source = source;
        }
        protected override void apply(TransformationServices services)
        {
            _apply(services,_source);
        }

        protected override bool commit()
        {
            return _source.Commit();
        }

        public override void Abort()
        {
            _source.RollBack();
        }
    }
}
