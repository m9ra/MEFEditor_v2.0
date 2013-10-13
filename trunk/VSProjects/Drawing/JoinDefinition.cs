using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drawing
{
    public class JoinDefinition
    {
        private readonly DrawingProperties _joinProperties = new DrawingProperties();

        public readonly JoinPointDefinition From;
        public readonly JoinPointDefinition To;

        public IEnumerable<DrawingProperty> Properties { get { return _joinProperties.Values ; } }

        public JoinDefinition(JoinPointDefinition from, JoinPointDefinition to)
        {
            From = from;
            To = to;
        }

        public void AddProperty(DrawingProperty property)
        {
            _joinProperties.Add(property.Name,property);
        }
    }
}
