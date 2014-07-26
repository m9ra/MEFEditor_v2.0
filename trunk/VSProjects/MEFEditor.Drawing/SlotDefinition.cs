using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Drawing
{
    /// <summary>
    /// Encapsulates drawings belonging to drawing definition. Is used for
    /// drawing children of containers for e.g.
    /// </summary>
    public class SlotDefinition
    {
        private readonly List<DrawingReference> _references = new List<DrawingReference>();

        public IEnumerable<DrawingReference> References { get { return _references; } }

        public void Add(DrawingReference reference)
        {
            _references.Add(reference);
        }
    }
}
