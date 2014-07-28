using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Drawing;
using MEFEditor.Analyzing;

using MEFEditor.TypeSystem;
using MEFEditor.TypeSystem.Runtime;
using MEFEditor.TypeSystem.DrawingServices;

using MEFEditor.Analyzing.Editing;

using RecommendedExtensions.Core.TypeDefinitions.CompositionEngine;

namespace RecommendedExtensions.Core.TypeDefinitions
{
    /// <summary>
    /// Analyzing definition of composition tester - that is used in MEFEditor.TestConsole application.
    /// </summary>
    public class CompositionTesterDefinition : DataTypeDefinition
    {
        /// <summary>
        /// The assembly path.
        /// </summary>
        protected Field<string> AssemblyPath;

        /// <summary>
        /// The parts.
        /// </summary>
        protected Field<List<Instance>> Parts;

        /// <summary>
        /// The composed flag.
        /// </summary>
        protected Field<bool> Composed;

        /// <summary>
        /// The composition result.
        /// </summary>
        protected Field<CompositionResult> CompositionResult;

        /// <summary>
        /// The composition context.
        /// </summary>
        protected Field<CompositionContext> CompositionContext;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CompositionTesterDefinition" /> class.
        /// </summary>
        public CompositionTesterDefinition()
        {
            FullName = "CompositionTester";
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        /// <param name="part1">The part1.</param>
        /// <param name="part2">The part2.</param>
        public void _method_ctor(Instance part1, Instance part2)
        {
            var compositionContext = new CompositionContext(Services, Context);
            compositionContext.AddConstructedComponents(new Instance[] { part1, part2 });

            compose(compositionContext);
            AssemblyPath.Set("Undefined");
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        public void _method_ctor(string assemblyPath)
        {
            _method_ctor();
            AssemblyPath.Set(assemblyPath);
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        public void _method_ctor()
        {
            Parts.Set(new List<Instance>());
            var thisObj = This;
            AddCallEdit(".accept", (s) => acceptComponent(thisObj, s));
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
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

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        /// <param name="part">The part.</param>
        public void _method_Add(Instance part)
        {
            ReportChildAdd(1, "Composition part");
            Parts.Get().Add(part);
        }

        /// <summary>
        /// Composes the specified composition context.
        /// </summary>
        /// <param name="compositionContext">The composition context.</param>
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

        /// <summary>
        /// Export data from represented <see cref="Instance" /> by using given drawer.
        /// <remarks>Note that only instances which are forced to display are displayed in root of <see cref="DiagramCanvas" /></remarks>.
        /// </summary>
        /// <param name="drawer">The drawer.</param>
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

        /// <summary>
        /// Gets the connector.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="drawer">The drawer.</param>
        /// <returns>ConnectorDefinition.</returns>
        private ConnectorDefinition getConnector(JoinPoint point, InstanceDrawer drawer)
        {
            var instance = drawer.GetInstanceDrawing(point.Instance.Component);

            var connector = instance.GetJoinPoint(point.Point);
            return connector;
        }

        /// <summary>
        /// Accepts the component.
        /// </summary>
        /// <param name="thisObj">The this object.</param>
        /// <param name="services">The services.</param>
        /// <returns>CallEditInfo.</returns>
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
