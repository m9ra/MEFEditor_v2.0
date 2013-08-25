using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Editing
{
    public class Edit
    {
        public readonly string Name;

        public readonly Transformation Transformation;

        internal bool IsEmpty { get { return Transformation is EmptyTransformation; } }

        public Edit(string name, Transformation transformation)
        {
            Name = name;
            Transformation = transformation;
        }
    }
}
