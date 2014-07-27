using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing.Editing;

namespace MEFEditor.Analyzing.Execution
{
    public class DirectInstance : Instance
    {
        private object _directValue;

        private readonly Machine _machine;

        public override object DirectValue { get { return _directValue; } }


        internal DirectInstance(object directValue, InstanceInfo info, Machine creatingMachine)
            : base(info)
        {
            _directValue = directValue;
            _machine = creatingMachine;
        }

        public override string ToString()
        {
            if (DirectValue != null)
            {
                return string.Format("[{0}]{1}", Info.TypeName, DirectValue.ToString());
            }
            else
            {
                return base.ToString();
            }
        }

        internal void Initialize(object data)
        {
            _directValue = data;
        }
    }
}
