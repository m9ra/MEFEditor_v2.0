using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution
{
    class DataInstance : Instance
    {
        readonly Dictionary<string, object> _fields = new Dictionary<string, object>();

        readonly InstanceInfo _info;

        public override object DirectValue
        {
            get { throw new NotImplementedException(); }
        }

        internal DataInstance(InstanceInfo info)
            : base(info)
        {
            _info = info;
        }

        internal void SetField(string fieldName, object value)
        {
            _fields[fieldName] = value;
        }

        internal object GetField(string fieldName)
        {
            return _fields[fieldName];
        }

        public override string ToString()
        {
            return string.Format("[Data]{0}", _info);
        }
    }
}
