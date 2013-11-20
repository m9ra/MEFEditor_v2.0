using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Editing.Transformations
{
    class EmptyTransformation : Transformation
    {
        protected override void apply()
        {
            View.Abort("There is missing transformation for requested operation");
        }
    }
}
