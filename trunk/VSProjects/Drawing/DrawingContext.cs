using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drawing
{
    public class DrawingContext
    {
        private readonly List<DrawingDefinition> _definitions = new List<DrawingDefinition>();

        public IEnumerable<DrawingDefinition> Definitions { get { return _definitions; } }

        public int Count { get { return _definitions.Count; } }

        public void Add(DrawingDefinition drawing)
        {
            _definitions.Add(drawing);
        }
    }
}
