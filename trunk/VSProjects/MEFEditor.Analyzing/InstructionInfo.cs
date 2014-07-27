using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing.Editing;

namespace MEFEditor.Analyzing
{
    public class InstructionInfo
    {
        private BlockTransformProvider _shiftingProvider;


        /// <summary>
        /// Comment message for instructions including this info
        /// </summary>
        public string Comment;

        /// <summary>
        /// Identifier that is used for limiting shifting changes
        /// only to scope of same block group
        /// </summary>
        public readonly object GroupID;
        
        public BlockTransformProvider BlockTransformProvider
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

        internal InstructionInfo(object groupID)
        {
            GroupID = groupID;
        }
    }
}
