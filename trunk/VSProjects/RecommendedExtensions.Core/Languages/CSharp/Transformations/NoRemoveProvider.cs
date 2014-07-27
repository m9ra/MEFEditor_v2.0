using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MEFEditor.Analyzing.Editing;
using MEFEditor.Analyzing.Editing.Transformations;

namespace RecommendedExtensions.Core.Languages.CSharp.Transformations
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
