using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Editing;
using Analyzing.Execution.Instructions;

namespace Analyzing
{
    public class CallBuilder
    {
        Call _call;
        internal CallBuilder(Call call)
        {
            _call = call;
            _call.TransformProvider = new EmptyCallTransformProvider();
        }

        public void SetTransformationProvider(CallTransformProvider transformProvider)
        {
            _call.TransformProvider = transformProvider;
        }
    }
}
