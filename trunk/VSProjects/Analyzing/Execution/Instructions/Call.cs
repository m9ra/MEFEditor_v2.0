using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Editing;

namespace Analyzing.Execution.Instructions
{
    class Call : InstructionBase
    {
        private readonly MethodID _method;

        private readonly Arguments _arguments;

        internal CallTransformProvider TransformProvider { get; set; }

        internal Call(MethodID methodGeneratorName, Arguments arguments)
        {
            _method = methodGeneratorName;
            _arguments = arguments;
        }

        public override void Execute(AnalyzingContext context)
        {
            var argumentValues = context.GetArguments(_arguments);
            InstanceInfo[] dynamicInfo = null;
            if (_method.NeedsDynamicResolving)
            {
                dynamicInfo = new InstanceInfo[argumentValues.Length];
                for (int i = 0; i < dynamicInfo.Length; ++i)
                {
                    dynamicInfo[i] = argumentValues[i].Info;
                }
            }

            var generator = context.GetGenerator(_method, dynamicInfo);
            context.FetchCallInstructions(_method, generator, argumentValues);
        }

        public override string ToString()
        {
            return string.Format("prepare_call {0}\ncall {1}", _arguments, _method);
        }
    }
}
