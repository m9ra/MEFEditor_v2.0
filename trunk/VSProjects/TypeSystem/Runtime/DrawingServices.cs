using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Drawing;

using Analyzing;

namespace TypeSystem.Runtime
{
    public class DrawingServices
    {
        private readonly HashSet<Instance> _needDrawingInstances = new HashSet<Instance>();

        internal IEnumerable<Instance> DependencyInstances { get { return _needDrawingInstances; } }

        internal readonly Instance DrawedInstance;

        public readonly DiagramItemDefinition Drawing;

        public readonly DiagramDefinition Context;

        internal DrawingServices(Instance instance, DiagramDefinition context)
        {
            Drawing = new DiagramItemDefinition(instance.ID, instance.Info.TypeName);
            DrawedInstance = instance;
            Context = context;
        }

        public void PublishField(string name, Field field)
        {
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
            Context.Draw(Drawing);
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
