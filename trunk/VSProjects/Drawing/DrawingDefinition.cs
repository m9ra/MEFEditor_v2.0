using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drawing
{
    public class DrawingDefinition
    {
        Dictionary<string, DrawingProperty> _properties = new Dictionary<string, DrawingProperty>();

        public IEnumerable<DrawingProperty> Properties { get { return _properties.Values; } }

        public void SetProperty(string name, string value)
        {
            _properties[name] = new DrawingProperty(name, value); 
        }
    }
}
