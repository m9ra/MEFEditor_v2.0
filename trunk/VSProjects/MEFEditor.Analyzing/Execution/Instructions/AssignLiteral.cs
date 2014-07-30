using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing.Execution.Instructions
{
    class AssignLiteral : AssignBase
    {        
        private readonly VariableName _targetVariable;
        private readonly Instance _literal;

        internal AssignLiteral(VariableName targetVariable, Instance literal)
        {
            _literal = literal;
            _targetVariable = targetVariable;
        }

        public override void Execute(AnalyzingContext context)
        {
            //literal can have another initial values from previous cached runs,
            //however literal behaves like it is created again so we will reset
            //previous flags.
            _literal.IsDirty = false;
            _literal.CreationBlock = context.CurrentCall.CurrentBlock;

            context.SetValue(_targetVariable, _literal);
        }

        public override string ToString()
        {
            return string.Format("mov_const {0}, {1}", _targetVariable, _literal);
        }
    }
}
