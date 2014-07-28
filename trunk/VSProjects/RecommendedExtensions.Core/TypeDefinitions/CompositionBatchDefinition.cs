using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

using MEFEditor.Drawing;
using MEFEditor.Analyzing;
using MEFEditor.Analyzing.Editing;
using MEFEditor.TypeSystem;
using MEFEditor.TypeSystem.Runtime;

using RecommendedExtensions.Core.TypeDefinitions.CompositionEngine;

namespace RecommendedExtensions.Core.TypeDefinitions
{
    /// <summary>
    /// Analyzing definition of <see cref="CompositionBatch" />.
    /// </summary>
    public class CompositionBatchDefinition : DataTypeDefinition
    {
        /// <summary>
        /// Publicly exposed type descriptor of current definition.
        /// </summary>
        public readonly static TypeDescriptor Info = TypeDescriptor.Create<CompositionBatch>();

        /// <summary>
        /// The parts to add.
        /// </summary>
        protected readonly Field<List<Instance>> PartsToAdd;

        /// <summary>
        /// The parts to remove.
        /// </summary>
        protected readonly Field<List<Instance>> PartsToRemove;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositionBatchDefinition" /> class.
        /// </summary>
        public CompositionBatchDefinition()
        {
            Simulate<CompositionBatch>();

            AddCreationEdit("Add CompositionBatch");
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        public void _method_ctor()
        {
            PartsToAdd.Value = new List<Instance>();
            PartsToRemove.Value = new List<Instance>();

            AddCallEdit(UserInteraction.AcceptEditName, acceptComponent);
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        /// <param name="part">The part.</param>
        public void _method_AddPart(Instance part)
        {
            ReportChildAdd(1, "Part to add");
            PartsToAdd.Value.Add(part);
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        /// <param name="part">The part.</param>
        public void _method_RemovePart(Instance part)
        {
            ReportChildAdd(1, "Part to remove");
            PartsToRemove.Value.Add(part);
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        /// <returns>Instance[].</returns>
        public Instance[] _get_PartsToAdd()
        {
            return PartsToAdd.Value.ToArray();
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        /// <returns>Instance[].</returns>
        public Instance[] _get_PartsToRemove()
        {
            return PartsToRemove.Value.ToArray();
        }

        /// <summary>
        /// Accepts the component.
        /// </summary>
        /// <param name="view">The view where component will be accepted.</param>
        /// <returns>CallEditInfo.</returns>
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

        /// <summary>
        /// Export data from represented <see cref="Instance" /> by using given drawer.
        /// <remarks>Note that only instances which are forced to display are displayed in root of <see cref="MEFEditor.Drawing.DiagramCanvas" /></remarks>.
        /// </summary>
        /// <param name="drawer">The drawer.</param>
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
