using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

using Analyzing;
using Analyzing.Editing;
using TypeSystem;
using TypeSystem.Runtime;

using Drawing;

using MEFAnalyzers.CompositionEngine;

namespace MEFAnalyzers
{
    public class CompositionContainerDefinition : DataTypeDefinition
    {
        protected Field<Instance> ComposableCatalog;
        protected Field<Instance[]> ComposedParts;
        protected Field<CompositionResult> CompositionResult;
        protected Field<Edit> ComposePartsCreate;


        public CompositionContainerDefinition()
        {
            Simulate<CompositionContainer>();
            AddCreationEdit("Add CompositionContainer", Dialogs.VariableName.GetName);
        }

        /// <summary>
        /// TODO ComposablePartCatalog restriction is needed
        /// </summary>
        /// <param name="composablePartCatalog"></param>
        public void _method_ctor(Instance composablePartCatalog)
        {
            initEdits(false);

            ReportChildAdd(1, "Composable part catalog", true);
            ComposableCatalog.Set(composablePartCatalog);
        }

        public void _method_ctor()
        {
            initEdits(true);
        }

        public void _method_ComposeParts(params Instance[] constructedParts)
        {
            //there is already compose part call 
            Edits.Remove(ComposePartsCreate.Get());
            //so we will accept components to this call
            var e = Edits;
            AppendArg(constructedParts.Length + 1, UserInteraction.AcceptName, (v) => acceptAppendComponent(e, v));


            //collect instances for composition
            var catalog = ComposableCatalog.Get();

            ComposedParts.Set(constructedParts);
            for (int i = 0; i < constructedParts.Length; ++i)
            {
                ReportParamChildAdd(i + 1, constructedParts[i], "Composed part", true);
            }

            if (catalog == null)
            {
                //there is no catalog which parts can be retrieved
                processComposition(null, constructedParts);
            }
            else
            {
                AsyncCall<Instance[]>(catalog, "get_Parts", (catalogParts) =>
                {
                    processComposition(catalogParts, constructedParts);
                });
            }
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

        #region Container edits

        private void initEdits(bool acceptCatalog)
        {
            //TODO provide edit context
            var e = Edits;
            if (acceptCatalog)
            {
                AppendArg(1, UserInteraction.AcceptName, (v) => acceptPartCatalog(e, v));
            }

            ComposePartsCreate.Set(
                AddCallEdit(UserInteraction.AcceptName, (v) => acceptComponent(v))
            );
        }

        private CallEditInfo acceptComponent(ExecutionView view)
        {
            //TODO check for using System.ComponentModel.Composition;
            var toAccept = UserInteraction.DraggedInstance;
            var componentInfo = Services.GetComponentInfo(toAccept.Info);

            if (componentInfo == null)
            {
                view.Abort("Can accept only components");
                return null;
            }

            return new CallEditInfo(This, "ComposeParts", toAccept);
        }

        private object acceptAppendComponent(EditsProvider e, ExecutionView view)
        {
            var toAccept = UserInteraction.DraggedInstance;
            var componentInfo = Services.GetComponentInfo(toAccept.Info);

            if (componentInfo == null)
            {
                view.Abort("Can accept only components");
                return null;
            }
            return e.GetVariableFor(toAccept, view);
        }

        private object acceptPartCatalog(EditsProvider e, ExecutionView view)
        {
            var instance = UserInteraction.DraggedInstance;
            var isCatalog = Services.IsAssignable(InstanceInfo.Create<ComposablePartCatalog>(), instance.Info);

            if (!isCatalog)
            {
                //allow accepting only components
                view.Abort("CompositionContainer can only accept part catalogs");
                return null;
            }

            return e.GetVariableFor(instance, view);
        }

        #endregion

        #region Container drawing

        protected override void draw(InstanceDrawer drawer)
        {
            var slot = drawer.AddSlot();

            drawCatalog(drawer, slot);
            setCompositionInfo(drawer);

            drawer.ForceShow();
        }

        private void setCompositionInfo(InstanceDrawer drawer)
        {
            var compositionResult = CompositionResult.Get();

            CompositionContext context = null;
            if (compositionResult != null)
            {
                context = compositionResult.Context;
                drawer.SetProperty("Error", compositionResult.Error);
            }

            if (context != null)
            {
                foreach (var component in context.InputInstances)
                {
                    var drawing = drawer.GetInstanceDrawing(component);
                    drawing.SetProperty("Composed", "True");
                }

                foreach (var point in compositionResult.Points)
                {
                    var joinPoint = getConnector(point, drawer);
                }

                foreach (var join in compositionResult.Joins)
                {
                    if (compositionResult.Failed && !join.IsErrorJoin)
                        //on error we want to display only error joins
                        continue;

                    var fromPoint = getConnector(join.Export, drawer);
                    var toPoint = getConnector(join.Import, drawer);

                    var joinDefinition = drawer.DrawJoin(fromPoint, toPoint);
                    //TODO set properties of joinDefinition
                }
            }
        }

        private void drawCatalog(InstanceDrawer drawer, SlotDefinition slot)
        {
            var composableCatalog = ComposableCatalog.Get();
            if (composableCatalog != null)
            {
                var catalogDrawing = drawer.GetInstanceDrawing(composableCatalog);
                slot.Add(catalogDrawing.Reference);
            }

            var composedParts = ComposedParts.Get();
            if (composedParts != null)
            {
                foreach (var composedPart in composedParts)
                {
                    var partDrawing = drawer.GetInstanceDrawing(composedPart);
                    slot.Add(partDrawing.Reference);
                }
            }
        }

        private ConnectorDefinition getConnector(JoinPoint point, InstanceDrawer drawer)
        {
            var instance = drawer.GetInstanceDrawing(point.Instance.Component);

            var connector = instance.GetJoinPoint(point.Point);

            connector.SetProperty("Error", point.Error);
            connector.SetProperty("Warning", point.Warning);

            return connector;
        }

        #endregion
    }
}
