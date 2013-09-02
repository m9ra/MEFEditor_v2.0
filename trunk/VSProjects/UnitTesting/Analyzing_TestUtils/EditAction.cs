using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

namespace UnitTesting.Analyzing_TestUtils
{
    public class EditAction
    {
        public readonly VariableName Variable;

        public readonly string Name;

        public readonly bool IsRemoveAction;

        /// <summary>
        /// If name is null, it is an remove action
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="name"></param>
        private EditAction(VariableName variable, string name)
        {
            Variable = variable;
            Name = name;
            IsRemoveAction = name == null;
        }

        internal static EditAction Edit(VariableName variable, string actionName)
        {
            if (actionName == null)
                throw new ArgumentNullException("actionName");

            var action = new EditAction(variable, actionName);
            return action;
        }

        internal static EditAction Remove(VariableName variable)
        {
            var action = new EditAction(variable, null);
            return action;
        }
    }
}
