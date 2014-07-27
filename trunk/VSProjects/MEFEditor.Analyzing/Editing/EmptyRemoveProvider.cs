using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing.Editing.Transformations;

namespace MEFEditor.Analyzing.Editing
{
    class EmptyRemoveProvider:RemoveTransformProvider
    {
        public override Transformation Remove()
        {
            return new EmptyTransformation();
        }

        public override NavigationAction GetNavigation()
        {
            return null;
        }
    }
}
