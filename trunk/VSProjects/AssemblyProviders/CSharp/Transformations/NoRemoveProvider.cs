using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Analyzing.Editing;
using Analyzing.Editing.Transformations;

namespace AssemblyProviders.CSharp.Transformations
{
    class NoRemoveProvider:RemoveTransformProvider
    {
        public override NavigationAction GetNavigation()
        {
            return null;
        }

        public override Transformation Remove()
        {
            return new IdentityTransformation();
        }
    }
}
