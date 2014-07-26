using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Drawing
{
    public class DrawingProperty
    {
        public readonly string Name;

        public readonly string Value;

        public DrawingProperty(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
