using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Editing.Transformations;

namespace Analyzing.Editing
{
    public class Edit
    {
        public readonly string Name;

        public readonly Transformation Transformation;

        public readonly Instance Provider;

        public readonly EditsProvider Context;

        /// <summary>
        /// Instance which caused creation of edit
        /// <remarks>Note that creator can be different from provider because of edit attaching</remarks>
        /// </summary>
        public readonly Instance Creator;

        internal bool IsEmpty { get { return Transformation is EmptyTransformation; } }

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
