using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drawing
{
    public class DrawingContext
    {
        private readonly Dictionary<string,DrawingDefinition> _definitions = new Dictionary<string,DrawingDefinition>();

        public IEnumerable<DrawingDefinition> Definitions { get { return _definitions.Values; } }

        public int Count { get { return _definitions.Count; } }

        public void Add(DrawingDefinition drawing)
        {
            if (ContainsDrawing(drawing.ID))
                throw new NotSupportedException("Drawing definition with same ID has already been added");

            _definitions.Add(drawing.ID,drawing);
        }

        public bool ContainsDrawing(string id)
        {
            return _definitions.ContainsKey(id);
        }
    }
}
