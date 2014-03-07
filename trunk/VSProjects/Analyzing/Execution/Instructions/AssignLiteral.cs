using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution.Instructions
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
            context.SetValue(_targetVariable, _literal);
        }

        public override string ToString()
        {
            return string.Format("mov_const {0}, {1}", _targetVariable, _literal);
        }
    }
}
