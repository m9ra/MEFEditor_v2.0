using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Drawing;
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
            _method_ctor();
            AssemblyPath.Set(assemblyPath);
        }

        public void _method_ctor()
        {
            Parts.Set(new List<Instance>());
            var thisObj = This;
            AddCallEdit((s) => acceptComponent(thisObj, s));
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
            drawer.PublishField("AssemblyPath", AssemblyPath);
            drawer.PublishField("Composed", Composed);

            var slot = drawer.AddSlot();

            var context = CompositionContext.Get();
            if (context != null)
            {

                foreach (var component in CompositionContext.Get().InputInstances)
                {
                    var drawing = drawer.GetInstanceDrawing(component);
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

            drawer.CommitDrawing();
        }

        private ConnectorDefinition getConnector(JoinPoint point, InstanceDrawer drawer)
        {
            //TODO this should set component itself
            var compositionResult = CompositionResult.Get();
            var instance = drawer.GetInstanceDrawing(point.Instance.Component);


            var connector = instance.GetJoinPoint(point.Point);

            var kind = getConnectorKind(point);

            setProperty(connector, "Kind", kind);
            setProperty(connector, "Contract", point.Contract);
            setProperty(connector, "ContractType", point.ContractType.TypeName);
            setProperty(connector, "AllowMany", point.AllowMany);
            setProperty(connector, "AllowDefault", point.AllowDefault);
            setProperty(connector, "IsPrerequisity", point.IsPrerequesity);

            return connector;
        }

        private void setProperty(ConnectorDefinition connector, string propertyName, object propertyValue)
        {
            connector.SetProperty(new DrawingProperty(propertyName, propertyValue.ToString()));
        }

        private string getConnectorKind(JoinPoint point)
        {
            var pointType = point.Point.GetType();

            if (pointType == typeof(Import))
            {
                return "Import";
            }
            else if (pointType == typeof(Export))
            {
                return "Export";
            }
            else
            {
                throw new NotSupportedException("Unsupported point kind");
            }
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
