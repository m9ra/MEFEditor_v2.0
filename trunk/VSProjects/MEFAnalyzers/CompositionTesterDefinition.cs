using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Drawing;
using Analyzing;

using TypeSystem;
using TypeSystem.Runtime;
using TypeSystem.DrawingServices;

using Analyzing.Editing;

using MEFAnalyzers.CompositionEngine;

namespace MEFAnalyzers
{
    public class CompositionTesterDefinition : DataTypeDefinition
    {
        protected Field<string> AssemblyPath;
        protected Field<List<Instance>> Parts;
        protected Field<bool> Composed;
        protected Field<CompositionResult> CompositionResult;
        protected Field<CompositionContext> CompositionContext;

        public CompositionTesterDefinition()
        {
            FullName = "CompositionTester";
        }

        public void _method_ctor(Instance part1, Instance part2)
        {
            var compositionContext = new CompositionContext(Services, Context);
            compositionContext.AddConstructedComponents(new Instance[] { part1, part2 });

            compose(compositionContext);
            AssemblyPath.Set("Undefined");
        }

        public void _method_ctor(string assemblyPath)
        {
            _method_ctor();
            AssemblyPath.Set(assemblyPath);
        }

        public void _method_ctor()
        {
            Parts.Set(new List<Instance>());
            var thisObj = This;
            AddCallEdit(".accept", (s) => acceptComponent(thisObj, s));
        }

        public void _method_Compose()
        {
            var path = AssemblyPath.Get();
            var compositionContext = new CompositionContext(Services, Context);
            if (path != null)
            {
                var assembly = Services.LoadAssembly(path);
                var notConstructed = new List<Instance>();
                foreach (var componentInfo in assembly.GetComponents())
                {
                    var instance = Context.Machine.CreateInstance(componentInfo.ComponentType);
                    notConstructed.Add(instance);
                }

                compositionContext.AddNotConstructedComponents(notConstructed);
            }

            var parts = Parts.Get();
            compositionContext.AddConstructedComponents(parts);
            compose(compositionContext);
        }

        public void _method_Add(Instance part)
        {
            ReportChildAdd(1, "Composition part");
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

        protected override void draw(InstanceDrawer drawer)
        {
            var isComposed = Composed.Get();
            drawer.PublishField("AssemblyPath", AssemblyPath);
            drawer.PublishField("Composed", Composed);

            var slot = drawer.AddSlot();

            var context = CompositionContext.Get();
            if (context != null)
            {

                foreach (var component in CompositionContext.Get().InputInstances)
                {
                    var drawing = drawer.GetInstanceDrawing(component);
                    drawing.SetProperty("Composed", isComposed.ToString());
                    slot.Add(drawing.Reference);
                }

                var compositionResult = CompositionResult.Get();

                foreach (var point in compositionResult.Points)
                {
                    var joinPoint = getConnector(point, drawer);
                }

                foreach (var join in compositionResult.Joins)
                {
                    var fromPoint = getConnector(join.Export, drawer);
                    var toPoint = getConnector(join.Import, drawer);

                    var joinDefinition = drawer.DrawJoin(fromPoint, toPoint);
                    //TODO set properties of joinDefinition
                }
            }

            drawer.ForceShow();
        }

        private ConnectorDefinition getConnector(JoinPoint point, InstanceDrawer drawer)
        {
            var instance = drawer.GetInstanceDrawing(point.Instance.Component);

            var connector = instance.GetJoinPoint(point.Point);
            return connector;
        }

        private CallEditInfo acceptComponent(Instance thisObj, ExecutionView services)
        {
            var instance = UserInteraction.DraggedInstance;
            var componentInfo = Services.GetComponentInfo(instance.Info);
            if (componentInfo == null)
            {
                //allow accepting only components
                services.Abort("CompositionTester can only accept components");
                return null;
            }

            return new CallEditInfo(thisObj, "Add", instance);
        }
    }
}
