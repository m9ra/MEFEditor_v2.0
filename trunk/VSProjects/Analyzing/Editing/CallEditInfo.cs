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
        public readonly object ThisObj;
        public readonly string CallName;
        public readonly object[] CallArguments;

        public CallEditInfo(object thisObj, string callName, params object[] callArgs)
        {
            ThisObj = thisObj;
            CallName = callName;
            CallArguments = callArgs;
        }

        public IEnumerable<Instance> Instances
        {
            get
            {
                if (ThisObj is Instance)
                    yield return ThisObj as Instance;

                foreach (var arg in CallArguments)
                    if (arg is Instance)
                        yield return arg as Instance;
            }
        }

        internal CallEditInfo Substitute(Dictionary<Instance, VariableName> substitutions)
        {
            var args = new List<object>(CallArguments.Length);
            foreach (var arg in CallArguments)
            {
                args.Add(subsitute(arg, substitutions));
            }

            return new CallEditInfo(
                subsitute(ThisObj, substitutions), CallName, args.ToArray()
                );
        }

        private object subsitute(object oldValue, Dictionary<Instance, VariableName> subsitutions)
        {
            var value = oldValue as Instance;
            if (value == null)
                return oldValue;

            return subsitutions[value];
        }
    }
}
