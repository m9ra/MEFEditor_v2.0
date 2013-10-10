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
        public readonly DrawingDefinition Drawing;

        public readonly DrawingContext Context;

        internal readonly Instance DrawedInstance;

        internal DrawingServices(Instance instance, DrawingContext context)
        {
            Drawing = new DrawingDefinition();
            DrawedInstance = instance;
            Context = context;
        }

        public void PublishField(string name, Field field)
        {
            Drawing.SetProperty(name, field.RawObject.ToString());
        }

        public void CommitDrawing()
        {
            Context.Add(Drawing);
        }
    }
}
