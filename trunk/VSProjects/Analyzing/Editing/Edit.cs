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

        internal readonly Instance Provider;

        internal bool IsEmpty { get { return Transformation is EmptyTransformation; } }

        public Edit(Instance provider, string name, Transformation transformation)
        {
            Name = name;
            Transformation = transformation;
            Provider = provider;
        }
    }
}
