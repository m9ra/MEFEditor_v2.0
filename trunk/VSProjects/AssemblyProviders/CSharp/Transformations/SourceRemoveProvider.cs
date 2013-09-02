using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Editing;

namespace AssemblyProviders.CSharp.Transformations
{
    class SourceRemoveProvider : RemoveTransformProvider
    {
        readonly TransformAction _action;
        readonly Source _source;
        internal SourceRemoveProvider(TransformAction action,Source source)
        {
            if (action == null)
                throw new ArgumentNullException();
            _action = action;
            _source = source;
        }
        public override Transformation Remove()
        {
            return new SourceTransformation(_action,_source);
        }
    }
}
