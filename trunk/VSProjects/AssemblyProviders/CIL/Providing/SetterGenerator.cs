using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using Analyzing.Execution;

namespace AssemblyProviders.CIL.Providing
{
    class SetterGenerator : GeneratorBase
    {
        private readonly string _fieldName;

        public SetterGenerator(string fieldName)
        {
            _fieldName = fieldName;
        }

        protected override void generate(EmitterBase emitter)
        {
            emitter.DirectInvoke(set);
        }

        private void set(AnalyzingContext context)
        {
            var This = context.CurrentArguments[0];
            var value = context.CurrentArguments[1];
            context.SetField(This, _fieldName, value);
        }
    }
}
