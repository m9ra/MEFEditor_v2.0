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
        Field<string> AssemblyPath;
        Field<List<Instance>> Parts;

        public CompositionTesterDefinition()
        {            
            FullName = "CompositionTester";
            AssemblyPath = new Field<string>(this);
            Parts = new Field<List<Instance>>(this);
        }

        public void _method_ctor(Instance part1, Instance part2)
        {
            var compositionContext = new CompositionContext(Services);
            compositionContext.AddConstructedComponents(part1, part2);

            compose(compositionContext);
        }

        public void _method_ctor(string assemblyPath)
        {
            Parts.Set(new List<Instance>());
            AssemblyPath.Set(assemblyPath);            
        }

        public void _method_Compose()
        {
            var path = AssemblyPath.Get();
            var assembly=Services.LoadAssembly(path);

            var compositionContext = new CompositionContext(Services);
            foreach (var part in Parts.Get())
            {
                compositionContext.AddConstructedComponents(part);
            }

            foreach (var componentInfo in assembly.GetComponents())
            {
                compositionContext.AddComponentType(componentInfo);
            }

            compose(compositionContext);
        }

        public void _method_Add(Instance part)
        {
            Parts.Get().Add(part);
        }

        private void compose(CompositionContext compositionContext)
        {
            var composition = CompositionProvider.Compose(compositionContext);

            if (!composition.Failed)
            {
                Context.DynamicCall("$dynamic_composition", composition.Generator, compositionContext.InputInstances);
            }
        }

    }
}
