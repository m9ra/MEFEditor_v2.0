using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing.Editing
{
    public abstract class RemoveTransformProvider
    {
        public abstract NavigationAction GetNavigation();

        public abstract Transformation Remove();
    }
}
