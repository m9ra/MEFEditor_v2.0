using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Drawing
{
    public class ConnectorDefinition
    {
        private readonly DrawingProperties _properties = new DrawingProperties();

        public readonly DrawingReference Reference;

        public IEnumerable<DrawingProperty> Properties { get { return _properties.Values; } }

        public ConnectorDefinition(DrawingReference reference)
        {
            Reference = reference;
        }

        public void SetProperty(DrawingProperty property)
        {
            _properties[property.Name] = property;
        }

        public void SetProperty(string name, string value)
        {
            SetProperty(new DrawingProperty(name, value));
        }

        public DrawingProperty GetProperty(string name)
        {
            DrawingProperty result;
            _properties.TryGetValue(name, out result);
            return result;
        }

        public string GetPropertyValue(string name)
        {
            var property = GetProperty(name);
            if (property == null)
                return null;

            return property.Value;
        }
    }
}
