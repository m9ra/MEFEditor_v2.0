using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drawing
{
    public class ConnectorDefinition
    {
        public readonly DrawingReference Reference;

        public ConnectorDefinition(DrawingReference reference)
        {
            Reference = reference;
        }
    }
}
