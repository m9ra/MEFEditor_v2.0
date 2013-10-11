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
        private readonly HashSet<Instance> _needDrawingInstances=new HashSet<Instance>();

        internal IEnumerable<Instance> DependencyInstances { get { return _needDrawingInstances; } }

        internal readonly Instance DrawedInstance;

        public readonly DrawingDefinition Drawing;

        public readonly DrawingContext Context;

        internal DrawingServices(Instance instance, DrawingContext context)
        {
            Drawing = new DrawingDefinition(instance.ID,instance.Info.TypeName);
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
            Context.Add(Drawing);
        }

        /// <summary>
        /// Force drawing of given instance. Its reference can be used
        /// in drawing slot.
        /// </summary>
        /// <param name="instance">Instance to be drawed</param>
        /// <returns>Drawing reference for displaying in drawing slot</returns>
        public DrawingReference Draw(Instance instance)
        {
            //TODO enqueue instance to need definition queue
            return new DrawingReference(instance.ID);
        }
    }
}
