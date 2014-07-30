using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Drawing;

using MEFEditor.Analyzing;
using MEFEditor.Analyzing.Editing;

using MEFEditor.TypeSystem.Runtime;

namespace MEFEditor.TypeSystem.DrawingServices
{
    /// <summary>
    /// Representation of instance that will be drawn at diagram.
    /// It provides services for creating <see cref="DiagramItemDefinition" />.
    /// </summary>
    public class DrawedInstance
    {
        /// <summary>
        /// The pipeline processing general and concrete drawing.
        /// </summary>
        internal readonly DrawingPipeline Pipeline;

        /// <summary>
        /// Gets the <see cref="DiagramDefinition"/> in which context
        /// will be current instance drawn.
        /// </summary>
        /// <value>The context.</value>
        internal DiagramDefinition Context { get { return Pipeline.Context; } }

        /// <summary>
        /// Concrete instance drawer for current instance.
        /// </summary>
        internal readonly InstanceDrawer InstanceDrawer;

        /// <summary>
        /// Gets the reference to drawing of current instance.
        /// </summary>
        /// <value>The reference.</value>
        public DrawingReference Reference { get { return Drawing.Reference; } }

        /// <summary>
        /// The instance which drawing is created.
        /// </summary>
        public readonly Instance WrappedInstance;

        /// <summary>
        /// The created drawing definition.
        /// </summary>
        public readonly DiagramItemDefinition Drawing;

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawedInstance" /> class.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="pipeline">The pipeline.</param>
        internal DrawedInstance(RuntimeTypeDefinition definition, Instance instance, DrawingPipeline pipeline)
        {
            Pipeline = pipeline;
            WrappedInstance = instance;

            Drawing = new DiagramItemDefinition(instance.ID, instance.Info.TypeName);
            InstanceDrawer = new InstanceDrawer(this);

            addEdits();
        }


        /// <summary>
        /// Publishes value the field.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="field">The field.</param>
        internal void PublishField(string name, Field field)
        {
            if (field.RawObject == null)
                return;

            Drawing.SetProperty(name, field.RawObject.ToString());
        }

        /// <summary>
        /// Adds the slot to drawing.
        /// </summary>
        /// <returns>SlotDefinition.</returns>
        internal SlotDefinition AddSlot()
        {
            var slot = new SlotDefinition();
            Drawing.AddSlot(slot);

            return slot;
        }

        /// <summary>
        /// Commits the drawing.
        /// </summary>
        internal void CommitDrawing()
        {
            Pipeline.DrawItem(this, Drawing);
        }

        /// <summary>
        /// Gets the connector definition defined for given key.
        /// </summary>
        /// <param name="pointKey">The point key.</param>
        /// <returns>ConnectorDefinition.</returns>
        public ConnectorDefinition GetJoinPoint(object pointKey)
        {
            return Context.DrawJoinPoint(Reference, pointKey);
        }

        /// <summary>
        /// Sets value of property with given name.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="value">The value.</param>
        public void SetProperty(string name, string value)
        {
            Drawing.SetProperty(name, value);
        }

        /// <summary>
        /// Adds all instance edits to drawing definition.
        /// </summary>
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
