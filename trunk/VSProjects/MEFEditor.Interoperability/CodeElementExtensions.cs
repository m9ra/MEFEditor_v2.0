using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnvDTE;

using System.Runtime.InteropServices;

namespace MEFEditor.Interoperability
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
        
        public static CodeElements Children(this CodeElement element)
        {
            switch (element.Kind)
            {
                case vsCMElement.vsCMElementNamespace:
                    return (element as CodeNamespace).Members;                
                default:
                    return element.Children;
            }
        }

        /// <summary>
        /// Get <see cref="CodeElement"/> from Parent property. Considers only named code constructs.
        /// </summary>
        /// <param name="element">Element which parent is needed</param>
        /// <returns>Parent of given element</returns>
        public static CodeElement Parent(this CodeElement element)
        {
            var parent = getParent(element) as CodeElement;
            return parent;
        }

        /// <summary>
        /// Get non-caseted <see cref="CodeElement"/> from Parent property. Considers only named code constructs.
        /// </summary>
        /// <param name="e">Element which parent is needed</param>
        /// <returns>Parent of given element</returns>
        private static object getParent(CodeElement e)
        {
            switch (e.Kind)
            {
                case vsCMElement.vsCMElementClass:
                    return (e as CodeClass).Parent;

                case vsCMElement.vsCMElementFunction:
                    return (e as CodeFunction).Parent;

                case vsCMElement.vsCMElementVariable:
                    return (e as CodeVariable).Parent;

                case vsCMElement.vsCMElementInterface:
                    return (e as CodeInterface).Parent;

                case vsCMElement.vsCMElementNamespace:
                    return (e as CodeNamespace).Parent;

                case vsCMElement.vsCMElementParameter:
                    return (e as CodeParameter).Parent;

                case vsCMElement.vsCMElementAttribute:
                    return (e as CodeAttribute).Parent;

                default:
                    //parent is not defined
                    return null;
            }
        }
    }
}
