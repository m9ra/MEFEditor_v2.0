using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing.Editing.Transformations;

namespace MEFEditor.Analyzing.Editing
{
    /// <summary>
    /// Representation of <see cref="ExecutionView" /> edit.
    /// </summary>
    public class Edit
    {
        /// <summary>
        /// The name of current edit.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The transformation defined by current edit.
        /// </summary>
        public readonly Transformation Transformation;

     
        /// <summary>
        /// The context where current edit was created.
        /// </summary>
        public readonly EditsProvider Context;

        /// <summary>
        /// Instance which caused creation of edit
        /// <remarks>Note that creator can be different from provider because of edit attaching</remarks>.
        /// </summary>
        public readonly Instance Creator;

        /// <summary>
        /// The provider of current edit.
        /// </summary>
        public readonly Instance Provider;

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        internal bool IsEmpty { get { return Transformation is EmptyTransformation; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="Edit" /> class.
        /// </summary>
        /// <param name="creator">The creator of edit.</param>
        /// <param name="provider">The provider of edit.</param>
        /// <param name="editContext">The edit context.</param>
        /// <param name="name">The name of current edit.</param>
        /// <param name="transformation">The transformation defined by current edit.</param>
        public Edit(Instance creator, Instance provider, EditsProvider editContext, string name, Transformation transformation)
        {
            Creator = creator;
            Name = name;
            Transformation = transformation;
            Provider = provider;
            Context = editContext;
        }
    }
}
