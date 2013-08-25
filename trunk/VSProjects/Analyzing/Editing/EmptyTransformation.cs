using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Editing
{
    class EmptyTransformation : Transformation
    {
        protected override void apply(TransformationServices services)
        {
            //nothing to do
        }
    }
}
