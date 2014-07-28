using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing.Editing
{
    /// <summary>
    /// Class RemoveTransformProvider.
    /// </summary>
    public abstract class RemoveTransformProvider
    {
        /// <summary>
        /// Gets the navigation.
        /// </summary>
        /// <returns>NavigationAction.</returns>
        public abstract NavigationAction GetNavigation();

        /// <summary>
        /// Removes this instance.
        /// </summary>
        /// <returns>Transformation.</returns>
        public abstract Transformation Remove();
    }
}
