using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Editing;

namespace Analyzing.Execution
{
    public class DirectInstance : Instance
    {
        private readonly object _directValue;
        public override object DirectValue { get { return _directValue; } }


        internal DirectInstance(object directValue, InstanceInfo info)
            : base(info)
        {
            _directValue = directValue;
        }

        public override string ToString()
        {
            if (DirectValue != null)
            {
                return string.Format("[{0}]{1}", DirectValue.GetType(), DirectValue.ToString());
            }
            else
            {
                return base.ToString();
            }
        }



    }
}
