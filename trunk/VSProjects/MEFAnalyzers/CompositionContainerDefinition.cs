using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Composition.Hosting;

using Analyzing;
using TypeSystem;
using TypeSystem.Runtime;

using Drawing;

using MEFAnalyzers.CompositionEngine;

namespace MEFAnalyzers
{
    public class CompositionContainerDefinition : DataTypeDefinition
    {
        Field<Instance> ComposableCatalog;
        Field<CompositionResult> CompositionResult;

        public CompositionContainerDefinition()
        {
            Simulate<CompositionContainer>();

            ComposableCatalog = new Field<Instance>(this);
            CompositionResult = new Field<CompositionResult>(this);
        }

        /// <summary>
        /// TODO ComposablePartCatalog restriction is needed
        /// </summary>
        /// <param name="composablePartCatalog"></param>
        public void _method_ctor(Instance composablePartCatalog)
        {
            ComposableCatalog.Set(composablePartCatalog);
        }

        public void _method_ComposeParts()
        {
            var catalog = ComposableCatalog.Get();
            var constructedParts = new Instance[0];

            AsyncCall<Instance[]>(catalog, "get_Parts", (catalogParts) =>
            {
                processComposition(catalogParts, constructedParts);
            });
        }

        /// <summary>
        /// TODO composition batch restriction is needed
        /// </summary>
        public void _method_Compose(Instance batch)
        {
            throw new NotImplementedException();
        }

        private void processComposition(IEnumerable<Instance> notConstructed, IEnumerable<Instance> constructed)
        {
            var composition = new CompositionContext(Services, Context);

            //add components that needs importing constructor call
            composition.AddNotConstructedComponents(notConstructed);

            //add instances that doesn't need constructor call
            composition.AddConstructedComponents(constructed);

            //create composition
            var compositionResult = CompositionProvider.Compose(composition);
            CompositionResult.Set(compositionResult);

            if (!compositionResult.Failed)
            {
                //if composition is OK then process composition
                Context.DynamicCall("$dynamic_composition", composition.Generator, composition.InputInstances);
            }
        }

        protected override void draw(InstanceDrawer drawer)
        {
            var slot = drawer.AddSlot();

            var compositionResult = CompositionResult.Get();
            var context = compositionResult.Context;


            if (context != null)
            {
                foreach (var component in context.InputInstances)
                {
                    var drawing = drawer.GetInstanceDrawing(component);
                    drawing.SetProperty("Composed", "True");
                    slot.Add(drawing.Reference);
                }
                                
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

            drawer.CommitDrawing();
        }

        private ConnectorDefinition getConnector(JoinPoint point, InstanceDrawer drawer)
        {
            var instance = drawer.GetInstanceDrawing(point.Instance.Component);

            var connector = instance.GetJoinPoint(point.Point);
            return connector;
        }
    }
}
