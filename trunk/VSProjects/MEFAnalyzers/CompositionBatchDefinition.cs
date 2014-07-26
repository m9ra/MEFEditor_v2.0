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

using MEFEditor.Drawing;

using MEFAnalyzers.CompositionEngine;

namespace MEFAnalyzers
{
    public class CompositionBatchDefinition : DataTypeDefinition
    {
        public readonly static TypeDescriptor Info = TypeDescriptor.Create<CompositionBatch>();

        protected readonly Field<List<Instance>> PartsToAdd;

        protected readonly Field<List<Instance>> PartsToRemove;

        public CompositionBatchDefinition()
        {
            Simulate<CompositionBatch>();

            AddCreationEdit("Add CompositionBatch");
        }

        public void _method_ctor()
        {
            PartsToAdd.Value = new List<Instance>();
            PartsToRemove.Value = new List<Instance>();

            AddCallEdit(UserInteraction.AcceptEditName, acceptComponent);
        }

        public void _method_AddPart(Instance part)
        {
            ReportChildAdd(1, "Part to add");
            PartsToAdd.Value.Add(part);
        }

        public void _method_RemovePart(Instance part)
        {
            ReportChildAdd(1, "Part to remove");
            PartsToRemove.Value.Add(part);
        }

        public Instance[] _get_PartsToAdd()
        {
            return PartsToAdd.Value.ToArray();
        }

        public Instance[] _get_PartsToRemove()
        {
            return PartsToRemove.Value.ToArray();
        }

        private CallEditInfo acceptComponent(ExecutionView view)
        {
            var toAccept = UserInteraction.DraggedInstance;
            var componentInfo = Services.GetComponentInfo(toAccept.Info);

            if (componentInfo == null)
            {
                view.Abort("Can accept only components");
                return null;
            }

            return new CallEditInfo(This, "AddPart", toAccept);
        }

        protected override void draw(InstanceDrawer drawer)
        {
            var slot = drawer.AddSlot();
            foreach (var toAdd in PartsToAdd.Value)
            {
                var drawing=drawer.GetInstanceDrawing(toAdd);
                slot.Add(drawing.Reference);
            }

            foreach (var toRemove in PartsToRemove.Value)
            {
                var drawing = drawer.GetInstanceDrawing(toRemove);
                drawing.SetProperty("Removed", "by CompositionBatch");
                slot.Add(drawing.Reference);
            }

            drawer.ForceShow();
        }
    }
}
