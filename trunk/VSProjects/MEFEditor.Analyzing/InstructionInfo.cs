using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing.Editing;

namespace MEFEditor.Analyzing
{
    /// <summary>
    /// Represent information collected about block of instructions.
    /// Blocks are used for avoiding of transformations that are
    /// not possible in source instructions (single source instruction can be
    /// represented by multiple IAL instructions).
    /// </summary>
    public class InstructionInfo
    {
        /// <summary>
        /// Provider of shifting transformations on current instruction block.
        /// </summary>
        private BlockTransformProvider _shiftingProvider;

        /// <summary>
        /// Comment message for instructions included in current instruction block.
        /// </summary>
        public string Comment;

        /// <summary>
        /// Identifier that is used for limiting shifting changes
        /// only to scope of same block group.
        /// </summary>
        public readonly object GroupID;

        /// <summary>
        /// Gets or sets the provider of shifting transformations on current instruction block.
        /// </summary>
        /// <value>The block transform provider.</value>
        /// <exception cref="System.NotSupportedException">Cannot set shifting provider twice</exception>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="InstructionInfo" /> class.
        /// </summary>
        /// <param name="groupID">The group identifier.</param>
        internal InstructionInfo(object groupID)
        {
            GroupID = groupID;
        }
    }
}
