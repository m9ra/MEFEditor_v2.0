using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Editing.Transformations
{
    /// <summary>
    /// Transformation that doesnt change view - identity
    /// </summary>
    public class IdentityTransformation : Transformation
    {
        /// <inheritdoc />        
        protected override void apply()
        {
            //no transformation
        }
    }
}
