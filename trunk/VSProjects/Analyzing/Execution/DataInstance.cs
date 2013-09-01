using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Execution
{
    class DataInstance<InstanceInfo>:Instance
    {
        readonly Dictionary<string, Instance> _fields = new Dictionary<string, Instance>();

        readonly InstanceInfo _info;

        public override object DirectValue
        {
            get { throw new NotImplementedException(); }
        }

        internal DataInstance(InstanceInfo info)
        {
            _info = info;
        }

        internal void SetField(string fieldName, Instance value)
        {
            _fields[fieldName] = value;
        }

        internal Instance GetField(string fieldName)
        {
            return _fields[fieldName];
        }

        public override string ToString()
        {
            return string.Format("[Data]{0}", _info);
        }
    }
}
