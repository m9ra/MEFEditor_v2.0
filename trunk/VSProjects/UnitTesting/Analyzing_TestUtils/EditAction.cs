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

        public readonly object Value;


        public EditAction(VariableName variable, string name, object value)
        {
            Variable = variable;
            Name = name;
            Value = value;
        }
    }
}
