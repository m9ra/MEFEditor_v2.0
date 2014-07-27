using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;

namespace MEFEditor.TypeSystem
{
    public class DirectGenerator:GeneratorBase, GenericMethodGenerator
    {
        private readonly DirectMethod _method;

        public DirectGenerator(DirectMethod directMethod)
        {
            _method = directMethod;
        }

        protected override void generate(EmitterBase emitter)
        {
            emitter.DirectInvoke(_method);
        }

        public MethodItem Make(PathInfo methodPath, TypeMethodInfo methodDefinition)
        {
            return new MethodItem(this, methodDefinition.MakeGenericMethod(methodPath));
        }
    }
}
