using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using Analyzing.Execution;

namespace AssemblyProviders.CIL.Providing
{
    class GetterGenerator : GeneratorBase
    {
        private readonly string _fieldName;

        public GetterGenerator(string fieldName)
        {
            _fieldName = fieldName;
        }

        protected override void generate(EmitterBase emitter)
        {
            emitter.DirectInvoke(get);
        }

        private void get(AnalyzingContext context)
        {
            var This = context.CurrentArguments[0];

            var fieldValue = context.GetField(This, _fieldName) as Instance;

            if (fieldValue == null)
                context.Return(null);

            context.Return(fieldValue);
        }
    }
}
