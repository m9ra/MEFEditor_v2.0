using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnvDTE;

namespace Interoperability
{
    public static class CodeElementExtensions
    {
        /// <summary>
        /// element kinds, which are watched for changes
        /// </summary>
        static readonly HashSet<vsCMElement> _watchedElements = new HashSet<vsCMElement>()
        {
           vsCMElement.vsCMElementClass,
           vsCMElement.vsCMElementFunction,
           vsCMElement.vsCMElementInterface,
           vsCMElement.vsCMElementNamespace,
           vsCMElement.vsCMElementEnum,
           vsCMElement.vsCMElementStruct,                
        };

        /// <summary>
        /// these elements doesnt have any interesting children to watch
        /// </summary>
        static readonly HashSet<vsCMElement> _noWatchedChildren = new HashSet<vsCMElement>()
        {
            vsCMElement.vsCMElementEnum,
            vsCMElement.vsCMElementFunction
        };

        /// <summary>
        /// Determine if element is watched for changes
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool IsWatched(this CodeElement element)
        {
            return _watchedElements.Contains(element.Kind);
        }

        /// <summary>
        /// Determine if element could have children that has to be watched
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool NoWatchedChildren(this CodeElement element)
        {
            if (element == null) return true;
            return _noWatchedChildren.Contains(element.Kind);
        }
    }
}
