using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Editing.Transformations
{
    class EmptyTransformation : Transformation
    {
        protected override void apply(TransformationServices services)
        {
            services.Abort("There is missing transformation for requested operation");
        }

        protected override bool commit()
        {
            //cannot commit empty transaction
            return false;   
        }
    }
}
