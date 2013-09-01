using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Editing;    

namespace Analyzing
{
    public abstract class Instance
    {
        readonly List<Edit> _edits = new List<Edit>();

        public bool IsDirty { get; private set; }

        public IEnumerable<Edit> Edits { get { return _edits; } }

        public abstract object DirectValue { get; }

        internal void AddEdit(Edit edit)
        {
            if (!edit.IsEmpty)
            {
                _edits.Add(edit);
            }
        }
    }
}
