using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Drawing;
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
        Field<bool> Composed;
        Field<CompositionResult> CompositionResult;
        Field<CompositionContext> CompositionContext;

        public CompositionTesterDefinition()
        {
            FullName = "CompositionTester";
            AssemblyPath = new Field<string>(this);
            Parts = new Field<List<Instance>>(this);
            Composed = new Field<bool>(this);
            CompositionContext = new Field<CompositionContext>(this);
            CompositionResult = new Field<CompositionResult>(this);
        }

        public void _method_ctor(Instance part1, Instance part2)
        {
            var compositionContext = new CompositionContext(Services, Context);
            compositionContext.AddConstructedComponents(part1, part2);

            compose(compositionContext);
            AssemblyPath.Set("Undefined");
        }

        public void _method_ctor(string assemblyPath)
        {
            Parts.Set(new List<Instance>());
            AssemblyPath.Set(assemblyPath);
        }

        public void _method_ctor()
        {
            Parts.Set(new List<Instance>());
        }


        public void _method_Compose()
        {
            var path = AssemblyPath.Get();
            var compositionContext = new CompositionContext(Services, Context);
            if (path != null)
            {
                var assembly = Services.LoadAssembly(path);
                foreach (var componentInfo in assembly.GetComponents())
                {
                    compositionContext.AddComponentType(componentInfo);
                }
            }

            foreach (var part in Parts.Get())
            {
                compositionContext.AddConstructedComponents(part);
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
            CompositionResult.Set(composition);
            CompositionContext.Set(compositionContext);
            if (!composition.Failed)
            {
                Context.DynamicCall("$dynamic_composition", composition.Generator, compositionContext.InputInstances);
                Composed.Set(true);
            }
        }

        protected override void draw(DrawingServices services)
        {
            services.PublishField("AssemblyPath", AssemblyPath);
            services.PublishField("Composed", Composed);

            var slot = services.AddSlot();

            foreach (var component in CompositionContext.Get().InputInstances)
            {
                var reference = services.Draw(component);
                slot.Add(reference);
            }

            var compositionResult = CompositionResult.Get();

            foreach (var point in compositionResult.Points)
            {
                var joinPoint = getJoinPoint(point, services);
                //TODO set join point properties
            }

            foreach (var join in compositionResult.Joins)
            {
                var fromPoint = getJoinPoint(join.Export, services);
                var toPoint = getJoinPoint(join.Import, services);

                var joinDefinition = services.DrawJoin(fromPoint, toPoint);
                //TODO set properties of joinDefinition
            }

            services.CommitDrawing();
        }

        private ConnectorDefinition getJoinPoint(JoinPoint point, DrawingServices services)
        {
            var compositionResult = CompositionResult.Get();
            var instance = point.Instance.Component;
            return services.GetJoinPoint(instance, point.Point);
        }
    }
}
