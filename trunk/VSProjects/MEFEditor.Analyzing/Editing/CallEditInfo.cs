using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing.Editing
{
    /// <summary>
    /// Provide definition of call creation edit.
    /// </summary>
    /// <param name="view">View where edit is processed.</param>
    /// <returns>Info used for instance accepting</returns>
    public delegate CallEditInfo CallProvider(ExecutionView view);

    /// <summary>
    /// Definition of call creation edit.
    /// </summary>
    public class CallEditInfo
    {
        /// <summary>
        /// Object which will be called.
        /// </summary>
        public readonly object ThisObj;

        /// <summary>
        /// The call name.
        /// </summary>
        public readonly string CallName;

        /// <summary>
        /// The call arguments.
        /// </summary>
        public readonly object[] CallArguments;

        /// <summary>
        /// Determine if call is an extension method.
        /// </summary>
        public readonly bool IsExtensionCall;

        /// <summary>
        /// Name of variable that will contain call return value.
        /// </summary>
        public string ReturnName;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallEditInfo" /> class.
        /// </summary>
        /// <param name="thisObj">Object which will be called.</param>
        /// <param name="callName">Name of the call.</param>
        /// <param name="callArgs">The call arguments.</param>
        public CallEditInfo(object thisObj, string callName, params object[] callArgs)
        {
            ThisObj = thisObj;
            CallName = callName;
            CallArguments = callArgs;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CallEditInfo" /> class.
        /// </summary>
        /// <param name="thisObj">Object which will be called.</param>
        /// <param name="callName">Name of the call.</param>
        /// <param name="isExtensionCall">if set to <c>true</c> [is extension call].</param>
        /// <param name="callArgs">The call arguments.</param>
        public CallEditInfo(object thisObj, string callName, bool isExtensionCall, params object[] callArgs)
            : this(thisObj, callName, callArgs)
        {
            IsExtensionCall = isExtensionCall;
        }

        /// <summary>
        /// Gets instances that will be contained in call.
        /// </summary>
        /// <value>The instances.</value>
        internal IEnumerable<Instance> Instances
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

        /// <summary>
        /// Substitutes arguments according to specified substitutions.
        /// </summary>
        /// <param name="substitutions">The substitutions.</param>
        /// <returns>CallEditInfo.</returns>
        internal CallEditInfo Substitute(Dictionary<Instance, VariableName> substitutions)
        {
            var args = new List<object>(CallArguments.Length);
            foreach (var arg in CallArguments)
            {
                args.Add(subsitute(arg, substitutions));
            }

            var call = new CallEditInfo(
                subsitute(ThisObj, substitutions), CallName, IsExtensionCall, args.ToArray()
                );

            call.ReturnName = ReturnName;

            return call;
        }

        /// <summary>
        /// Subsitutes give value.
        /// </summary>
        /// <param name="oldValue">The value.</param>
        /// <param name="substitutions">The subsitutions.</param>
        /// <returns>System.Object.</returns>
        private object subsitute(object oldValue, Dictionary<Instance, VariableName> substitutions)
        {
            var value = oldValue as Instance;
            if (value == null)
                return oldValue;

            return substitutions[value];
        }
    }
}
