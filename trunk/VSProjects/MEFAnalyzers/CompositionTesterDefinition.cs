using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

using TypeSystem;
using TypeSystem.Runtime;

using MEFAnalyzers.CompositionEngine;

namespace MEFAnalyzers
{
    public class CompositionTesterDefinition : DataTypeDefinition
    {
        public CompositionTesterDefinition()
        {
            FullName = "CompositionTester";
        }

        public void _method_ctor(Instance part1, Instance part2)
        {
            var compositionContext = new CompositionContext(Services);
            CompositionProvider.Compose(compositionContext, new Instance[] { part1, part2 });
        }
    }
}
