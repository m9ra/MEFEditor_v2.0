using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Drawing;

namespace TypeSystem.Runtime
{
    public class DrawingServices
    {
        public DrawingDefinition CurrentDrawing { get; internal set; }

        internal DrawingServices()
        {

        }

        public void PublishField(string name, Field field)
        {
            CurrentDrawing.SetProperty(name, field.RawObject.ToString());
        }
    }
}
