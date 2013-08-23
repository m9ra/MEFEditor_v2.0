using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing.Editing;
namespace AssemblyProviders.CSharp.Transformations
{
    class SourceTransformation:Transformation
    {
        readonly Action _apply;
        internal SourceTransformation(Action apply)
        {
            if (apply == null)
                throw new ArgumentNullException();
            _apply = apply;
        }
        public override void Apply()
        {
            _apply();
        }
    }
}
