using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Drawing;

using Analyzing;
using Analyzing.Editing;

namespace TypeSystem.Runtime
{
    public class DrawingServices
    {
        private readonly HashSet<Instance> _needDrawingInstances = new HashSet<Instance>();

        private readonly AnalyzingResult _result;

        internal IEnumerable<Instance> DependencyInstances { get { return _needDrawingInstances; } }

        internal readonly Instance DrawedInstance;

        public readonly DiagramItemDefinition Drawing;

        public readonly DiagramDefinition Context;



        internal DrawingServices(AnalyzingResult result, Instance instance, DiagramDefinition context)
        {
            _result = result;

            Drawing = new DiagramItemDefinition(instance.ID, instance.Info.TypeName);
            DrawedInstance = instance;
            Context = context;

            foreach (var edit in DrawedInstance.Edits)
            {
                var editDefinition = CreateEditDefinition(edit);
                Drawing.AddEdit(editDefinition);
            }

            foreach (var attachingInstance in DrawedInstance.AttachingInstances)
            {
                foreach (var attachedEdit in DrawedInstance.GetAttachedEdits(attachingInstance))
                {
                    var editDefinition = CreateEditDefinition(attachedEdit);
                    Drawing.AttachEdit(attachingInstance.ID, editDefinition);
                }
            }
        }

        public EditDefinition CreateEditDefinition(Edit edit)
        {
            return new EditDefinition(edit.Name, (preview) => runEdit(edit, preview), () => false);
        }

        private bool runEdit(Edit edit, bool preview)
        {
            var services = _result.CreateTransformationServices();
            services.Apply(edit.Transformation);
            if (services.IsAborted)
                return false;

            if (preview)
            {
                services.Abort("Preview");
            }
            else
            {
                services.Commit();
            }

            return true;
        }

        public void PublishField(string name, Field field)
        {
            if (field.RawObject == null)
                return;

            Drawing.SetProperty(name, field.RawObject.ToString());
        }

        public SlotDefinition AddSlot()
        {
            var slot = new SlotDefinition();
            Drawing.AddSlot(slot);

            return slot;
        }

        public void CommitDrawing()
        {
            Context.DrawItem(Drawing);
        }

        public ConnectorDefinition GetJoinPoint(Instance instance, object pointKey)
        {
            var instanceReference = getReference(instance);
            return Context.DrawJoinPoint(instanceReference, pointKey);
        }

        /// <summary>
        /// TODO better join point resolvings
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public JoinDefinition DrawJoin(ConnectorDefinition from, ConnectorDefinition to)
        {
            var join = new JoinDefinition(from, to);

            Context.DrawJoin(join);


            return join;
        }

        /// <summary>
        /// Force drawing of given instance. Its reference can be used
        /// in drawing slot.
        /// </summary>
        /// <param name="instance">Instance to be drawed</param>
        /// <returns>Drawing reference for displaying in drawing slot</returns>
        public DrawingReference Draw(Instance instance)
        {
            _needDrawingInstances.Add(instance);
            return getReference(instance);
        }

        private DrawingReference getReference(Instance instance)
        {
            return new DrawingReference(instance.ID);
        }
    }
}
