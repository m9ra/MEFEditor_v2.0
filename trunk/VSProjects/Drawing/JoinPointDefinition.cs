using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drawing
{
    public class JoinPointDefinition
    {
        public readonly DrawingReference Reference;

        public JoinPointDefinition(DrawingReference reference)
        {
            Reference = reference;
        }
    }
}
