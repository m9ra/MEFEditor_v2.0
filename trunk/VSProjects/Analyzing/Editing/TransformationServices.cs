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
        public string AbortMessage { get; private set; }

        public bool IsAborted { get { return AbortMessage != null; } }
            
        public object Abort(string abortMessage)
        {
            if (IsAborted)
            {
                throw new NotSupportedException("Cannot abort twice");
            }
            AbortMessage = abortMessage;

            return null;
        }

        public void Apply(Transformation transformation)
        {
            if (!IsAborted)
                transformation.Apply(this);
        }
    }
}
