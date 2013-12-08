using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Drawing;

using Analyzing;
using Analyzing.Editing;

using TypeSystem.Runtime;

namespace TypeSystem.DrawingServices
{
    public class DrawedInstance
    {
        internal readonly DrawingPipeline Pipeline;

        internal DiagramDefinition Context { get { return Pipeline.Context; } }

        internal readonly InstanceDrawer InstanceDrawer;

        public DrawingReference Reference { get { return Drawing.Reference; } }

        public readonly Instance WrappedInstance;

        public readonly DiagramItemDefinition Drawing;

        internal DrawedInstance(RuntimeTypeDefinition definition, Instance instance, DrawingPipeline pipeline)
        {
            Pipeline = pipeline;
            WrappedInstance = instance;

            Drawing = new DiagramItemDefinition(instance.ID, instance.Info.TypeName);
            InstanceDrawer = new InstanceDrawer(definition, this);

            addEdits();
        }


        internal void PublishField(string name, Field field)
        {
            if (field.RawObject == null)
                return;

            Drawing.SetProperty(name, field.RawObject.ToString());
        }

        internal SlotDefinition AddSlot()
        {
            var slot = new SlotDefinition();
            Drawing.AddSlot(slot);

            return slot;
        }

        internal void CommitDrawing()
        {
            Pipeline.Context.DrawItem(Drawing);
        }

        public ConnectorDefinition GetJoinPoint(object pointKey)
        {
            return Context.DrawJoinPoint(Reference, pointKey);
        }

        public void SetProperty(string name, string value)
        {
            Drawing.SetProperty(name, value);
        }


        private void addEdits()
        {
            foreach (var edit in WrappedInstance.Edits)
            {
                var editDefinition = InstanceDrawer.CreateEditDefinition(edit);
                Drawing.AddEdit(editDefinition);
            }

            foreach (var attachingInstance in WrappedInstance.AttachingInstances)
            {
                foreach (var attachedEdit in WrappedInstance.GetAttachedEdits(attachingInstance))
                {
                    var editDefinition = InstanceDrawer.CreateEditDefinition(attachedEdit);
                    Drawing.AttachEdit(attachingInstance.ID, editDefinition);
                }
            }
        }

    }
}
