using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Drawing
{
    /// <summary>
    /// Definition of <see cref="SlotCanvas"/> that can be used as slot filled by <see cref="DiagramItem"/>.    
    /// </summary>
    public class SlotDefinition
    {
        /// <summary>
        /// References of contained <see cref="DiagramItem"/> defnitions.
        /// </summary>
        private readonly List<DrawingReference> _references = new List<DrawingReference>();

        /// <summary>
        /// Gets references of contained <see cref="DiagramItem"/> defnitions.
        /// </summary>
        /// <value>The references.</value>
        public IEnumerable<DrawingReference> References { get { return _references; } }

        /// <summary>
        /// Adds <see cref="DiagramItem"/> to slot according to
        /// given reference.
        /// </summary>
        /// <param name="reference">The reference.</param>
        public void Add(DrawingReference reference)
        {
            _references.Add(reference);
        }
    }
}
