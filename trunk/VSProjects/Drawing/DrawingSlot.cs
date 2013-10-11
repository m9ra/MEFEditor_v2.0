using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drawing
{
    /// <summary>
    /// Encapsulates drawings belonging to drawing definition. Is used for
    /// drawing children of containers for e.g.
    /// </summary>
    public class DrawingSlot
    {
        private readonly List<DrawingReference> _references = new List<DrawingReference>();

        public void Add(DrawingReference reference)
        {
            _references.Add(reference);
        }
    }
}
