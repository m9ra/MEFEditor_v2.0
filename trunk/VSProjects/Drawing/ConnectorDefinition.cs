using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drawing
{
    public class ConnectorDefinition
    {
        private readonly DrawingProperties _properties = new DrawingProperties();

        public readonly DrawingReference Reference;

        public ConnectorDefinition(DrawingReference reference)
        {
            Reference = reference;
        }

        public void SetProperty(DrawingProperty property)
        {
            _properties[property.Name] = property;  
        }

        public DrawingProperty GetProperty(string name)
        {
            DrawingProperty result;
            _properties.TryGetValue(name, out result);
            return result;  
        }
    }
}
