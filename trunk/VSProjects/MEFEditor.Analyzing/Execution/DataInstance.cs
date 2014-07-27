using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing.Execution
{
    public class DataInstance : Instance
    {
        readonly Dictionary<string, object> _fields = new Dictionary<string, object>();

        readonly InstanceInfo _info;

        public override object DirectValue
        {
            get { throw new NotSupportedException("Only direct instances have direct value"); }
        }

        internal DataInstance(InstanceInfo info)
            : base(info)
        {
            _info = info;
        }

        public void SetField(string fieldName, object value)
        {
            _fields[fieldName] = value;
        }

        public object GetField(string fieldName)
        {
            object result;
            _fields.TryGetValue(fieldName, out result);
            return result;  
        }

        public override string ToString()
        {
            return string.Format("[Data]{0}", _info.TypeName);
        }
    }
}
