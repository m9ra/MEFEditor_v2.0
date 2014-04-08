using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnvDTE;

using TypeSystem;
using Interoperability;

namespace AssemblyProviders.ProjectAssembly.Traversing
{
    /// <summary>
    /// Searcher for getting <see cref="CodeElement"/> objects according to their path
    /// </summary>
    class CodeElementSearcher
    {
        /// <summary>
        /// Assembly where <see cref="CodeElement"/> objects are searched
        /// </summary>
        private readonly VsProjectAssembly _searchedAssembly;

        internal CodeElementSearcher(VsProjectAssembly searchedAssembly)
        {
            _searchedAssembly = searchedAssembly;
        }

        /// <summary>
        /// Search first <see cref="CodeElement"/> with given path.
        /// </summary>
        /// <param name="path">Search path</param>
        /// <returns>First <see cref="CodeElement"/> found with sufficient path</returns>
        internal CodeElement Search(string path)
        {
            var lazyEnumeration = SearchAll(path);

            if (!lazyEnumeration.Any())
                //no element satisfying given path has been found
                return null;

            return lazyEnumeration.First();
        }

        /// <summary>
        /// Search all <see cref="CodeElement"/> with given path.
        /// </summary>
        /// <param name="path">Search path</param>
        /// <returns>First <see cref="CodeElement"/> found with sufficient path</returns>
        internal IEnumerable<CodeElement> SearchAll(string path)
        {
            var pathInfo = new PathInfo(path);
            var pathParts = pathInfo.Signature.Split(Naming.PathDelimiter);
            if (pathParts.Length == 0)
                throw new NotSupportedException("Cannot find CodeElement with given path '" + path + "'");

            //resolve naming conventions of method paths

            //TODO resolve indexe    
            var isGetter = false;
            var isSetter = false;

            var lastPart = pathParts[pathParts.Length - 1];

            if (pathParts.Length > 1 && (lastPart == Naming.CtorName || lastPart == Naming.ClassCtorName))
            {
                //naming convention for ctors force to use class name
                lastPart = pathParts[pathParts.Length - 2];
            }
            else if (lastPart.StartsWith(Naming.GetterPrefix))
            {
                isGetter = true;
                lastPart = lastPart.Substring(Naming.GetterPrefix.Length);
            }
            else if (lastPart.StartsWith(Naming.SetterPrefix))
            {
                isSetter = true;
                lastPart = lastPart.Substring(Naming.SetterPrefix.Length);
            }

            //search for all children with given path on parentElement
            var parentElement = findElement(pathParts, pathParts.Length - 1);
            if (parentElement == null)
                //nothing has been found
                yield break;


            foreach (CodeElement child in parentElement.Children())
            {
                var name = child.Name();

                if (name == lastPart)
                {
                    var property = child as CodeProperty;

                    if (property == null)
                    {
                        yield return child;
                    }
                    else
                    {
                        //TODO check of correctnes for auto generated properties

                        if (isGetter)
                            yield return property.Getter as CodeElement;

                        if (isSetter)
                            yield return property.Setter as CodeElement;
                    }
                }
            }
        }

        /// <summary>
        /// Find <see cref="CodeElement"/> specified by first pathLength pathParts
        /// </summary>
        /// <param name="pathParts">Parth parts specifiing name of <see cref="CodeElement"/></param>
        /// <param name="pathLength">Length of prefix in pathParts used for <see cref="CodeElement"/> searching</param>
        /// <returns>Found <see cref="CodeElement"/> if any, <c>null</c> otherwise</returns>
        private CodeElement findElement(string[] pathParts, int pathLength)
        {
            var currentElements = _searchedAssembly.RootElements;

            //traverse all allowed part
            for (var i = 0; i < pathLength; ++i)
            {
                var isLast = pathLength == i + 1;
                var currentPart = pathParts[i];

                if (isLast && i > 1 && (currentPart == Naming.ClassCtorName || currentPart == Naming.CtorName))
                {
                    //change to constructor name
                    currentPart = pathParts[i - 1];
                }

                IEnumerable<CodeElement> nextElements = null;
                foreach (CodeElement currentChild in currentElements)
                {
                    var name = currentChild.Name();
                    if (name == currentPart)
                    {
                        if (isLast)
                            //we have found element satysfiing the path
                            return currentChild;

                        //current element satysfiing the path - step to its children
                        nextElements = from CodeElement child in currentChild.Children() select child;
                        //go to next part
                        break;
                    }
                }

                if (nextElements == null)
                    //no element matches current part
                    break;

                //shift to next children
                currentElements = nextElements;
            }

            //element hasn't been found
            return null;
        }
    }
}
