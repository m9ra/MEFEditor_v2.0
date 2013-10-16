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

        public readonly DrawingDefinition Drawing;

        public readonly DrawingContext Context;

        internal DrawingServices(Instance instance, DrawingContext context)
        {
            Drawing = new DrawingDefinition(instance.ID, instance.Info.TypeName);
            DrawedInstance = instance;
            Context = context;
        }

        public void PublishField(string name, Field field)
        {
            Drawing.SetProperty(name, field.RawObject.ToString());
        }

        public DrawingSlot AddSlot()
        {
            var slot = new DrawingSlot();
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
