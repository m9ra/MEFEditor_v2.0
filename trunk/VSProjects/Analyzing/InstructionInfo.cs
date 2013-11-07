using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Editing;

namespace Analyzing
{
    public class InstructionInfo
    {
        private BlockTransformationProvider _shiftingProvider;


        /// <summary>
        /// Comment message for instructions including this info
        /// </summary>
        public string Comment { get; set; }

        public BlockTransformationProvider ShiftingProvider
        {
            get
            {
                if (_shiftingProvider == null)
                {
                    _shiftingProvider = new EmptyShiftingProvider();
                }

                return _shiftingProvider;
            }

            set
            {
                if (_shiftingProvider != null)
                {
                    throw new NotSupportedException("Cannot set shifting provider twice");
                }

                _shiftingProvider = value;
            }
        }
    }
}
