using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Editing;

namespace Analyzing.Execution
{
    public class Instance
    {
        readonly Dictionary<string, Instance> _fields = new Dictionary<string, Instance>();
        readonly List<Edit> _edits = new List<Edit>();

        internal bool IsDirty { get; private set; }

        public object DirectValue { get; private set; }

        public IEnumerable<Edit> Edits { get { return _edits; } }


        internal Instance()
        {
        }

        internal Instance(object directValue)
        {
            DirectValue = directValue;
        }

        public override string ToString()
        {
            if (DirectValue != null)
            {
                return string.Format("[{0}]{1}",DirectValue.GetType(), DirectValue.ToString());
            }
            else
            {
                return base.ToString();
            }
        }

        internal void AddEdit(Edit edit)
        {
            if (!edit.IsEmpty)
            {
                _edits.Add(edit);
            }
        }

        internal void SetField(string fieldName, Instance value)
        {
            _fields[fieldName] = value;
        }

        internal Instance GetField(string fieldName)
        {
            return _fields[fieldName];
        }
    }
}
