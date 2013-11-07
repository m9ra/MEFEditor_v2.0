using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Editing
{
    /// <summary>
    /// Handle accepting instance according to given info
    /// </summary>
    /// <returns>Info used for instance accepting</returns>
    public delegate CallEditInfo CallProvider(TransformationServices services);

    public class CallEditInfo
    {
        internal readonly object ThisObj;
        internal readonly string CallName;
        internal readonly object[] CallArguments;

        public CallEditInfo(object thisObj, string callName, params object[] callArgs)
        {
            ThisObj = thisObj;
            CallName = callName;
            CallArguments = callArgs;
        }
    }
}
