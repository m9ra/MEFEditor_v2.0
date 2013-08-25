using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Execution;

namespace Analyzing.Editing
{
    /// <summary>
    /// Is available when transformation is applied to provide transformation services
    /// </summary>
    public class TransformationServices
    {
        public bool IsAborted { get; private set; }
            
        public object Abort(string abortMessage)
        {
            throw new NotImplementedException();
        }

        public void Apply(Transformation transformation)
        {
            if (!IsAborted)
                transformation.Apply(this);
        }
    }
}
